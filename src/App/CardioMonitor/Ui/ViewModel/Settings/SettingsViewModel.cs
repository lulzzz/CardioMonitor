using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Infrastructure.WpfCommon.Base;
using CardioMonitor.Resources;
using CardioMonitor.Settings;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.Logging;

namespace CardioMonitor.Ui.ViewModel.Settings
{
    public class SettingsViewModel : Notifier, IDataErrorInfo, IStoryboardPageViewModel
    {
        private readonly ICardioSettings _settings;
        private readonly ILogger _logger;
        private string _sessionsFilesDirectoryPath;
        private ICommand _chooseFolderCommand;
        private ICommand _closeCommand;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;
        private bool _isValid;
        private bool _isSettingsChanged;
        private string _dbServerName;
        private string _dbName;
        private string _dbLogin;
        private string _dbPassword;
        private string _dbPort;
        
        public string SessionsFilesDirectoryPath
        {
            get => _sessionsFilesDirectoryPath;
            set
            {
                if (value != _sessionsFilesDirectoryPath)
                {
                    _sessionsFilesDirectoryPath = value;
                    RisePropertyChanged(nameof(SessionsFilesDirectoryPath));
                    _isSettingsChanged = true;
                }
            }
        }

        #region Commands
        
        public ICommand ChooseFolderCommand
        {
            get
            {
                return _chooseFolderCommand ?? (_chooseFolderCommand = new SimpleCommand
                {
                    ExecuteDelegate = x => ChooseFolder()
                });
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new SimpleCommand
                {
                    ExecuteDelegate = x => CancelSettings(),
                    CanExecuteDelegate = x => _isSettingsChanged
                });
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => _isValid,
                    ExecuteDelegate = x => CloseSettings()
                });
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new SimpleCommand
                {
                    ExecuteDelegate = x => SaveSettings(),
                    CanExecuteDelegate = x => IsValid && _isSettingsChanged
                });
            }
        }

        #endregion


        public string DbServerName
        {
            get => _dbServerName;
            set
            {
                if (value != _dbServerName)
                {
                    _dbServerName = value;
                    RisePropertyChanged(nameof(DbServerName));
                    _isSettingsChanged = true;
                }
            }
        }


        public string DbPort
        {
            get => _dbPort;
            set
            {
                if (value != _dbPort)
                {
                    _dbPort = value;
                    RisePropertyChanged(nameof(DbPort));
                    _isSettingsChanged = true;
                }
            }
        }


        public string DbName
        {
            get => _dbName;
            set
            {
                if (value != _dbName)
                {
                    _dbName = value;
                    RisePropertyChanged(nameof(DbName));
                    _isSettingsChanged = true;
                }
            }
        }

        public string DbLogin
        {
            get => _dbLogin;
            set
            {
                if (value != _dbLogin)
                {
                    _dbLogin = value;
                    RisePropertyChanged(nameof(DbLogin));
                    _isSettingsChanged = true;
                }
            }
        }

        public string DbPassword
        {
            get => _dbPassword;
            set
            {
                if (value != _dbPassword)
                {
                    _dbPassword = value;
                    RisePropertyChanged(nameof(DbPassword));
                    _isSettingsChanged = true;
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RisePropertyChanged(nameof(IsBusy));
            }
        }
        private bool _isBusy;

        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value;
                RisePropertyChanged(nameof(BusyMessage));
            }
        }
        private string _busyMessage;


        public SettingsViewModel(
            ICardioSettings settings,
            [NotNull] ILogger logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void InitializeSettings()
        {
            SessionsFilesDirectoryPath = _settings.SessionsFilesDirectoryPath;
            //DbLogin = _settings.DataBaseSettings.User;
            //DbName = _settings.DataBaseSettings.DataBase;
            //DbPassword = _settings.DataBaseSettings.Password;
            //DbServerName = _settings.DataBaseSettings.Source;
            //DbPort = _settings.DataBaseSettings.Port;
            _isValid = true;
            _isSettingsChanged = false;
        }

        #region Validation

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(SessionsFilesDirectoryPath) && !Directory.Exists(SessionsFilesDirectoryPath))
                {
                    _isValid = false;
                    return Localisation.SettingsViewModel_DirectoryDoesNotExist;
                }
                if (columnName == nameof(DbLogin) 
                    || columnName == nameof(DbName)
                    || columnName == nameof(DbPassword)
                    || columnName == nameof(DbPassword))
                {
                    //todo need some realtime validation
                    /*try
                    {
                        DataBaseRepository.Instance.CheckConnection(DBName,DBServerName, DBLogin, DBPassword);
                    }
                    catch (Exception ex)
                    {
                        _isValid = false;
                        return ex.Message;
                    }*/
                }
                _isValid = true;
                return null;
            }
        }

        public string Error => String.Empty;

        public bool IsValid
        {
            get => _isValid;
            set
            {
                var oldValod = _isValid;
                _isValid = value;

                if (oldValod != value)
                {
                    RisePropertyChanged(nameof(IsValid));
                }
            }
        }

        #endregion

        public void ChooseFolder()
        {
            var folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = SessionsFilesDirectoryPath,
                Description = Localisation.SettingsViewModel_ChooseFolder
            };
            folderBrowserDialog.ShowDialog();
            SessionsFilesDirectoryPath = folderBrowserDialog.SelectedPath;
        }

        private async void SaveSettings()
        {
            try
            {

                IsBusy = true;
                BusyMessage = "Сохранение настроек";
                /* CardioSettings.Instance.SelectedAcentColorName = SelectedAccentColor.Name;
                CardioSettings.Instance.SeletedAppThemeName = SelectedAppTheme.Name;*/
                _settings.SessionsFilesDirectoryPath = SessionsFilesDirectoryPath;

                //_settings.DataBaseSettings = new DataBaseSettings(DbName, DbServerName, DbPort, DbLogin, DbPassword);

                var settingsManager = new SettingsManager();
                settingsManager.Save(_settings);

                _isSettingsChanged = false;
                await MessageHelper.Instance.ShowMessageAsync(Localisation.SettingsViewModel_Saved);
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка сохранения настроек. Причина: {e.Message}", e);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка сохранения настроек");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CloseSettings()
        {
            if (_isSettingsChanged)
            {
                InitializeSettings();
            }
        }

        private void CancelSettings()
        {
            InitializeSettings();
        }

        public void Dispose()
        {
        }

        #region StoryboardPageViewModel


        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            InitializeSettings();
            _isSettingsChanged = false;
            return Task.CompletedTask;
        }

        public Task<bool> CanLeaveAsync()
        {
            return Task.FromResult(true);
        }

        public Task LeaveAsync()
        {
            return Task.CompletedTask;
        }

        public Task ReturnAsync(IStoryboardPageContext context)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> CanCloseAsync()
        {
            if (_isSettingsChanged)
            {
                var result = await MessageHelper.Instance.ShowMessageAsync(
                    "Все несохраненные изменения будут потеряны. Вы уверены?", "Cardio Monitor",
                    MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false);
                return result == MessageDialogResult.Affirmative;
            }

            return true;
        }

        public Task CloseAsync()
        {
            return Task.CompletedTask;
        }

        public event Func<TransitionEvent, Task> PageCanceled;

        public event Func<TransitionEvent, Task> PageCompleted;

        public event Func<TransitionEvent, Task> PageBackRequested;

        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion

    }
}
