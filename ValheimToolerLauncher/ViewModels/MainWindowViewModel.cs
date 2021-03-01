using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using AutoUpdaterDotNET;
using ValheimToolerLauncher.Utils;
using System.Threading.Tasks;
using ValheimToolerLauncher.Properties;

namespace ValheimToolerLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private readonly Installer _installer;

        public RelayCommand InstallCommand { get; }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand LaunchCommand { get; }

        public RelayCommand UninstallCommand { get; }

        public RelayCommand BrowseCommand { get; }

        private UpdateInfoEventArgs _updateInfoEventArgs;
        public UpdateInfoEventArgs UpdateInfoEventArgs
        {
            get => _updateInfoEventArgs;
            set
            {
                Set(ref _updateInfoEventArgs, value);
            }
        }

        private bool _isInstalled;
        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                Set(ref _isInstalled, value);
                InstallCommand.RaiseCanExecuteChanged();
                LaunchCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isUpdateAvailable;
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                Set(ref _isUpdateAvailable, value);
                InstallCommand.RaiseCanExecuteChanged();
                LaunchCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
                BrowseCommand.RaiseCanExecuteChanged();
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private string _gamePath;
        public string GamePath
        {
            get => _gamePath;
            set
            {
                Set(ref _gamePath, value);
                InstallCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
            }
        }

        private Installer.InstallStatus _installStatus;
        public Installer.InstallStatus InstallStatus
        {
            get => _installStatus;
            set
            {
                _installStatus = value;
                InstallCommand.RaiseCanExecuteChanged();
                LaunchCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
            }
        }

        public MainWindowViewModel()
        {
            InstallCommand = new RelayCommand(ExecuteInstallCommand, CanExecuteInstallCommand);
            UpdateCommand = new RelayCommand(ExecuteUpdateCommand);
            LaunchCommand = new RelayCommand(ExecuteLaunchCommand, CanExecuteLaunchCommand);
            BrowseCommand = new RelayCommand(ExecuteBrowseCommand, CanExecuteBrowseCommand);
            UninstallCommand = new RelayCommand(ExecuteUninstallCommand, CanExecuteUninstallCommand);

            if (Settings.Default.updateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.updateSettings = false;
                Settings.Default.Save();

                _installer = new Installer();

                _installer.Upgrade();
            }
            else
            {
                _installer = new Installer();
            }

            InstallStatus = Installer.InstallStatus.IDLE;
            

            IsInstalled = _installer.IsInstalled;
            GamePath = _installer.GamePath;

            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.Start("https://www.codexus.fr/valheimtooler/AutoUpdater.xml");
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                IsUpdateAvailable = args.IsUpdateAvailable;
                if (args.IsUpdateAvailable)
                {
                    UpdateInfoEventArgs = args;
                }
            }
        }

        private void ExecuteUpdateCommand(object parameter)
        {
            AutoUpdater.RunUpdateAsAdmin = false;
            if (AutoUpdater.DownloadUpdate(UpdateInfoEventArgs))
            {
                App.Current.MainWindow.Close();
            }
        }

        private bool CanExecuteInstallCommand(object parameter)
        {
            return !IsInstalled && !IsUpdateAvailable && !string.IsNullOrEmpty(GamePath) && _installStatus == Installer.InstallStatus.IDLE;
        }

        private async void ExecuteInstallCommand(object parameter)
        {
            InstallStatus = Installer.InstallStatus.PROCESSING;

            await Task.Run(() =>
            {
                if (!_installer.Install())
                {
                    Status = $"Failed to install: {_installer.LastErrorMsg}";
                }
                else
                {
                    Status = "Install success!";
                }
            });

            IsInstalled = _installer.IsInstalled;

            InstallStatus = Installer.InstallStatus.IDLE;
        }

        private bool CanExecuteLaunchCommand(object parameter)
        {
            return IsInstalled && !IsUpdateAvailable && _installStatus == Installer.InstallStatus.IDLE;
        }

        private async void ExecuteLaunchCommand(object parameter)
        {
            MonoInjector injector = new MonoInjector();

            InstallStatus = Installer.InstallStatus.PROCESSING;

            await Task.Run(() =>
            {
                if (!injector.Inject())
                {
                    Status = $"Failed to inject: {injector.LastErrorMsg}";
                }
                else
                {
                    Status = "Injection done!";
                }
            });

            InstallStatus = Installer.InstallStatus.IDLE;
        }

        private bool CanExecuteBrowseCommand(object parameter)
        {
            return !IsUpdateAvailable && _installStatus == Installer.InstallStatus.IDLE;
        }

        private void ExecuteBrowseCommand(object parameter)
        {
            CommonOpenFileDialog openFolderDialog = new CommonOpenFileDialog
            {
                Title = "Select Valheim game path",
                IsFolderPicker = true
            };

            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (File.Exists(Path.Combine(openFolderDialog.FileName, "valheim.exe")))
                {
                    _installer.GamePath = openFolderDialog.FileName;
                    GamePath = _installer.GamePath;
                }
            }
        }

        private bool CanExecuteUninstallCommand(object parameter)
        {
            return IsInstalled && !IsUpdateAvailable && !string.IsNullOrEmpty(GamePath) && _installStatus == Installer.InstallStatus.IDLE;
        }

        private async void ExecuteUninstallCommand(object parameter)
        {
            InstallStatus = Installer.InstallStatus.PROCESSING;

            await Task.Run(() =>
            {
                if (!_installer.Uninstall())
                {
                    Status = $"Failed to uninstall: {_installer.LastErrorMsg}";
                }
                else
                {
                    Status = "Uninstall success!";
                }
            });

            IsInstalled = _installer.IsInstalled;

            InstallStatus = Installer.InstallStatus.IDLE;
        }
    }
}
