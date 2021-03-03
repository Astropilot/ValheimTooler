using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using AutoUpdaterDotNET;
using ValheimToolerLauncher.Utils;
using System.Threading.Tasks;
using ValheimToolerLauncher.Properties;
using ValheimToolerLauncher.Views;

namespace ValheimToolerLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private Installer _installer;

        public RelayCommand InstallCommand { get; }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand LaunchCommand { get; }

        public RelayCommand UninstallCommand { get; }

        public RelayCommand BrowseCommand { get; }

        public RelayCommand OnWindowLoadedCommand { get; }

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
                BrowseCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isIntegrityValid;
        public bool IsIntegrityValid
        {
            get => _isIntegrityValid;
            set
            {
                Set(ref _isIntegrityValid, value);
                InstallCommand.RaiseCanExecuteChanged();
                LaunchCommand.RaiseCanExecuteChanged();
                BrowseCommand.RaiseCanExecuteChanged();
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
                BrowseCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
            }
        }

        private Installer.InstallStatus _installStatus;
        public Installer.InstallStatus InstallStatus
        {
            get => _installStatus;
            set
            {
                Set(ref _installStatus, value);
                InstallCommand.RaiseCanExecuteChanged();
                LaunchCommand.RaiseCanExecuteChanged();
                BrowseCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isUpgrade = false;

        public MainWindowViewModel()
        {
            InstallCommand = new RelayCommand(ExecuteInstallCommand, CanExecuteInstallCommand);
            UpdateCommand = new RelayCommand(ExecuteUpdateCommand);
            LaunchCommand = new RelayCommand(ExecuteLaunchCommand, CanExecuteLaunchCommand);
            BrowseCommand = new RelayCommand(ExecuteBrowseCommand, CanExecuteBrowseCommand);
            UninstallCommand = new RelayCommand(ExecuteUninstallCommand, CanExecuteUninstallCommand);
            OnWindowLoadedCommand = new RelayCommand(ExecuteOnWindowLoadedCommand);

            InstallStatus = Installer.InstallStatus.PROCESSING;
        }

        private async void ExecuteOnWindowLoadedCommand(object parameter)
        {
            // Detect that we just updated the tool
            if (Settings.Default.updateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.updateSettings = false;
                Settings.Default.Save();
                _isUpgrade = true;
            }

            _installer = new Installer();

            Status = "Checking integrity...";
            var checkIntegrityResult = await Task.Run(() =>
            {
                return _installer.CheckBackupsIntegrity();
            });

            IsIntegrityValid = checkIntegrityResult;

            if (_isUpgrade)
            {
                Status = "Upgrading ValheimTooler...";
                await Task.Run(() =>
                {
                    _installer.Upgrade();
                });
                IsIntegrityValid = true;
            }

            IsInstalled = _installer.IsInstalled;
            GamePath = _installer.GamePath;

            Status = "Checking for update...";

            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.Start("https://www.codexus.fr/valheimtooler/AutoUpdater.xml");
        }

        private async void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                IsUpdateAvailable = args.IsUpdateAvailable;
                if (args.IsUpdateAvailable)
                {
                    UpdateInfoEventArgs = args;
                    InstallStatus = Installer.InstallStatus.IDLE;
                    return;
                }
            }

            if (!IsIntegrityValid && !_isUpgrade)
            {
                var integrityPoup = new IntegrityPopup();

                if (integrityPoup.ShowDialog() == true)
                {
                    await Task.Run(() =>
                    {
                        _installer.Upgrade();
                    });
                    IsInstalled = _installer.IsInstalled;
                    IsIntegrityValid = true;
                }
            }
            Status = "";
            InstallStatus = Installer.InstallStatus.IDLE;
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
            return !IsInstalled && !IsUpdateAvailable && !string.IsNullOrEmpty(GamePath) && _installStatus == Installer.InstallStatus.IDLE && IsIntegrityValid;
        }

        private async void ExecuteInstallCommand(object parameter)
        {
            InstallStatus = Installer.InstallStatus.PROCESSING;

            var installResult = await Task.Run(() =>
            {
                return _installer.Install();
            });

            if (installResult)
            {
                Status = "Install success!";
            }
            else
            {
                Status = $"Failed to install: {_installer.LastErrorMsg}";
            }

            IsInstalled = _installer.IsInstalled;

            InstallStatus = Installer.InstallStatus.IDLE;
        }

        private bool CanExecuteLaunchCommand(object parameter)
        {
            return IsInstalled && !IsUpdateAvailable && _installStatus == Installer.InstallStatus.IDLE && IsIntegrityValid;
        }

        private async void ExecuteLaunchCommand(object parameter)
        {
            MonoInjector injector = new MonoInjector();

            InstallStatus = Installer.InstallStatus.PROCESSING;

            var injectResult = await Task.Run(() =>
            {
                return injector.Inject();
            });

            if (injectResult)
            {
                Status = "Injection done!";
            }
            else
            {
                Status = $"Failed to inject: {injector.LastErrorMsg}";
            }

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

            var uninstallResult = await Task.Run(() =>
            {
                return _installer.Uninstall();
            });

            if (uninstallResult)
            {
                Status = "Uninstall success!";
            }
            else
            {
                Status = $"Failed to uninstall: {_installer.LastErrorMsg}";
            }

            IsInstalled = _installer.IsInstalled;

            InstallStatus = Installer.InstallStatus.IDLE;
        }
    }
}
