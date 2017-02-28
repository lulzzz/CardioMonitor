using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Resources;
using CardioMonitor.Settings;
using CardioMonitor.Ui.Base;

namespace CardioMonitor.Ui.ViewModel.Settings
{
    public class SettingsViewModel : Notifier, IDataErrorInfo
    {
        private readonly ICardioSettings _settings;
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

        #region Apperance settings
        /*
        private AppAppearanceSettings _selectedAccentColor;
        private AppAppearanceSettings _selectedAppTheme;
        public List<AppAppearanceSettings> AccentColors { get; set; }
        public List<AppAppearanceSettings> AppThemes { get; set; }

        public AppAppearanceSettings SelectedAccentColor
        {
            get { return _selectedAccentColor; }
            set
            {
                if (_selectedAccentColor != value)
                {
                    _selectedAccentColor = value;
                    RisePropertyChanged("SelectedAccentColor");

                    var theme = ThemeManager.DetectAppStyle(Application.Current);
                    var accent = ThemeManager.GetAccent(value.Name);
                    ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
                    _isSettingsChanged = true;
                }
            }
        }

        public AppAppearanceSettings SelectedAppTheme
        {
            get { return _selectedAppTheme; }
            set
            {
                if (_selectedAppTheme != value)
                {
                    _selectedAppTheme = value;
                    RisePropertyChanged("SelectedAppTheme");

                    var theme = ThemeManager.DetectAppStyle(Application.Current);
                    var appTheme = ThemeManager.GetAppTheme(value.Name);
                    ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
                    _isSettingsChanged = true;
                }
            }
        }*/
        #endregion

        public string SessionsFilesDirectoryPath
        {
            get { return _sessionsFilesDirectoryPath; }
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
            get { return _dbServerName; }
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

        public string DbName
        {
            get { return _dbName; }
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
            get { return _dbLogin; }
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
            get { return _dbPassword; }
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

        public SettingsViewModel(ICardioSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            _settings = settings;
            /*
            AccentColors = ThemeManager.Accents
                                            .Select(a => new AppAppearanceSettings { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                            .ToList();

            AppThemes = ThemeManager.AppThemes
                                           .Select(a => new AppAppearanceSettings { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                                           .ToList();*/
            InitializeSettings();
            _isSettingsChanged = false;
        }

        private void InitializeSettings()
        {
            /*SelectedAppTheme =
               AppThemes.FirstOrDefault(x => x.Name == CardioSettings.Instance.SeletedAppThemeName);
            SelectedAccentColor =
                AccentColors.FirstOrDefault(x => x.Name == CardioSettings.Instance.SelectedAcentColorName);*/
            SessionsFilesDirectoryPath = _settings.SessionsFilesDirectoryPath;
            DbLogin = _settings.DataBaseSettings.User;
            DbName = _settings.DataBaseSettings.DataBase;
            DbPassword = _settings.DataBaseSettings.Password;
            DbServerName = _settings.DataBaseSettings.Source;
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

        public string Error
        {
            get
            {
                return String.Empty;
            }
        }

        public bool IsValid
        {
            get { return _isValid; }
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

            /* CardioSettings.Instance.SelectedAcentColorName = SelectedAccentColor.Name;
            CardioSettings.Instance.SeletedAppThemeName = SelectedAppTheme.Name;*/
            _settings.SessionsFilesDirectoryPath = SessionsFilesDirectoryPath;

            _settings.DataBaseSettings = new DataBaseSettings(DbName, DbServerName, DbLogin, DbPassword);

            var settingsManager = new SettingsManager();
            settingsManager.Save(_settings);

            _isSettingsChanged = false;
            await MessageHelper.Instance.ShowMessageAsync(Localisation.SettingsViewModel_Saved);
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
    }
}
