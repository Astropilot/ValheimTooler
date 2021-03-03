using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ValheimToolerLauncher.Utils
{
    public class Installer
    {
        private const string Steam32RegistryKeyPath = @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam";
        private const string Steam64RegistryKeyPath = @"HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\Valve\Steam";

        private bool _isInstalled;
        private readonly string _launcherPath;
        private string _gamePath;

        public bool IsInstalled { get => _isInstalled; }

        public string LastErrorMsg { get; private set; }

        public string GamePath
        {
            get => _gamePath;
            set
            {
                _gamePath = value;
                if (!string.IsNullOrEmpty(value))
                {
                    Properties.Settings.Default.gamePath = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private Version _installedVersion;
        public Version InstalledVersion
        {
            get => _installedVersion;
            set
            {
                _installedVersion = value;
                Properties.Settings.Default.installedVersion = value != null ? value.ToString() : "";
                Properties.Settings.Default.Save();
            }
        }

        public Installer()
        {
            _gamePath = Properties.Settings.Default.gamePath;
            _isInstalled = !string.IsNullOrEmpty(Properties.Settings.Default.installedVersion); ;

            if (_isInstalled)
            {
                InstalledVersion = new Version(Properties.Settings.Default.installedVersion);
            }

            if (string.IsNullOrEmpty(_gamePath))
            {
                _gamePath = FindValheimGamePath();
            }

            _launcherPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public bool Install()
        {
            if (IsInstalled)
            {
                return DoError("Already installed");
            }

            string gameManagedPath = Path.Combine(GamePath, "valheim_Data", "Managed");
            string toolerManagedPath = Path.Combine(_launcherPath, "Managed");
            string gameManagedBackupPath = Path.Combine(_launcherPath, "Backups");

            if (!Directory.Exists(gameManagedPath))
            {
                return DoError("Game managed folder cannot be found!");
            }
            if (!Directory.Exists(toolerManagedPath))
            {
                return DoError("Managed folder cannot be found!");
            }
            if (Directory.Exists(gameManagedBackupPath))
            {
                try
                {
                    Directory.Delete(gameManagedBackupPath, true);
                }
                catch (Exception exc)
                {
                    return DoError(exc.Message);
                }
            }

            try
            {
                Directory.CreateDirectory(gameManagedBackupPath);

                CopyDir.CopyAll(new DirectoryInfo(gameManagedPath), new DirectoryInfo(gameManagedBackupPath));
                CopyDir.CopyAll(new DirectoryInfo(toolerManagedPath), new DirectoryInfo(gameManagedPath));
            }
            catch (Exception exc)
            {
                return DoError(exc.Message);
            }

            DirectoryInfo gameManagedDir = new DirectoryInfo(gameManagedPath);
            IEnumerable<FileInfo> gameManagedFiles = gameManagedDir.GetFiles("*.*", SearchOption.AllDirectories);
            var postInstallGameManagedFiles = new Dictionary<string, string>();

            File.Delete("post_install.chk");
            foreach (FileInfo file in gameManagedFiles)
            {
                postInstallGameManagedFiles.Add(FileUtils.GetRelativePath(file.FullName, gameManagedPath), file.ComputeHash());
            }

            var gameManagedHashesJson = JsonConvert.SerializeObject(postInstallGameManagedFiles);

            File.AppendAllText("post_install.chk", gameManagedHashesJson);

            InstalledVersion = Assembly.GetEntryAssembly().GetName().Version;

            _isInstalled = true;

            return true;
        }

        public bool CheckBackupsIntegrity()
        {
            if (!IsInstalled)
            {
                return true;
            }

            string gameManagedPath = Path.Combine(GamePath, "valheim_Data", "Managed");
            string gameManagedBackupPath = Path.Combine(_launcherPath, "Backups");
            DirectoryInfo gameManagedDir = new DirectoryInfo(gameManagedPath);
            IEnumerable<FileInfo> gameManagedFiles = gameManagedDir.GetFiles("*.*", SearchOption.AllDirectories);
            var postInstallContents = File.ReadAllText("post_install.chk");
            Dictionary<string, string> postInstallGameManagedFiles = JsonConvert.DeserializeObject<Dictionary<string, string>>(postInstallContents);
            bool isIntegrityValid = true;

            foreach (FileInfo gameManagedFile in gameManagedFiles)
            {
                var relativePath = FileUtils.GetRelativePath(gameManagedFile.FullName, gameManagedPath);

                if (!postInstallGameManagedFiles.ContainsKey(relativePath))
                {
                    // New file, copy to backup
                    string newPathInBackups = Path.Combine(gameManagedBackupPath, FileUtils.GetRelativePath(gameManagedFile.FullName, gameManagedPath));
                    gameManagedFile.CopyTo(newPathInBackups, true);
                    isIntegrityValid = false;
                }
                else
                {
                    string postInstallFileChecksum = postInstallGameManagedFiles[relativePath];
                    string gameManagedFileChecksum = gameManagedFile.ComputeHash();

                    if (postInstallFileChecksum != gameManagedFileChecksum)
                    {
                        // Modified file, copy to backup
                        string newPathInBackups = Path.Combine(gameManagedBackupPath, FileUtils.GetRelativePath(gameManagedFile.FullName, gameManagedPath));
                        gameManagedFile.CopyTo(newPathInBackups, true);
                        isIntegrityValid = false;
                    }
                }
            }

            foreach (string postInstallGameManagedFile in postInstallGameManagedFiles.Keys)
            {
                string pathInGameManaged = Path.Combine(gameManagedPath, postInstallGameManagedFile);

                if (!File.Exists(pathInGameManaged))
                {
                    // Deleted file, delete from backup too
                    string pathInManagedBackups = Path.Combine(gameManagedBackupPath, postInstallGameManagedFile);
                    File.Delete(pathInManagedBackups);
                    isIntegrityValid = false;
                }
            }

            return isIntegrityValid;
        }

        public bool Uninstall()
        {
            if (!IsInstalled)
            {
                return DoError("Not installed!");
            }

            string gameManagedPath = Path.Combine(GamePath, "valheim_Data", "Managed");
            string gameManagedBackupPath = Path.Combine(_launcherPath, "Backups");

            if (!Directory.Exists(gameManagedPath))
            {
                return DoError("Game managed folder cannot be found!");
            }
            if (!Directory.Exists(gameManagedBackupPath))
            {
                return DoError("No backups found!");
            }

            try
            {
                Directory.Delete(gameManagedPath, true);
                CopyDir.CopyAll(new DirectoryInfo(gameManagedBackupPath), new DirectoryInfo(gameManagedPath));

                Directory.Delete(gameManagedBackupPath, true);
            }
            catch (Exception exc)
            {
                return DoError(exc.Message);
            }

            File.Delete("post_install.chk");

            InstalledVersion = null;

            _isInstalled = false;

            return true;
        }

        public bool Upgrade()
        {
            if (!IsInstalled)
            {
                return DoError("No installed yet");
            }

            if (!Uninstall())
            {
                return DoError($"Failed to uninstall: {LastErrorMsg}");
            }
            if (!Install())
            {
                return DoError($"Failed to install: {LastErrorMsg}");
            }
            return true;
        }

        private bool DoError(string message)
        {
            LastErrorMsg = message;
            return false;
        }

        private string FindValheimGamePath()
        {
            var steamPath = Registry.GetValue(Steam32RegistryKeyPath, "SteamPath", "").ToString().Replace('/', Path.DirectorySeparatorChar);

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = Registry.GetValue(Steam64RegistryKeyPath, "SteamPath", "").ToString().Replace('/', Path.DirectorySeparatorChar);
            }

            if (string.IsNullOrEmpty(steamPath))
            {
                return null;
            }

            var gamePath = FindValheimGameInSteamPath(steamPath);

            if (!string.IsNullOrEmpty(gamePath))
            {
                return gamePath;
            }

            var libraryfoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(libraryfoldersPath))
            {
                return null;
            }

            var configFile = new Utils.KeyValue();
            configFile.ReadFileAsText(libraryfoldersPath);

            var steamGamePathConfig = configFile["1"];

            if (!string.IsNullOrEmpty(steamGamePathConfig.Value) && Directory.Exists(steamGamePathConfig.Value))
            {
                var steamGamePath = steamGamePathConfig.Value;

                return FindValheimGameInSteamPath(steamGamePath);
            }

            return gamePath;
        }

        private string FindValheimGameInSteamPath(string steamPath)
        {
            var appManifestPath = Path.Combine(steamPath, "steamapps", "appmanifest_892970.acf");

            if (File.Exists(appManifestPath))
            {
                return Path.Combine(steamPath, "steamapps", "common", "Valheim");
            }
            return null;
        }

        public enum InstallStatus
        {
            IDLE,
            PROCESSING
        }
    }
}
