using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Repository;
using CardioMonitor.Resources;
using CardioMonitor.Settings;
using CardioMonitor.Ui.Base;

namespace CardioMonitor.Ui.ViewModel.Settings
{
    public class SettingsViewModel : Notifier, IDataErrorInfo
    {
        private readonly DataBaseRepository _dataBaseRepository;
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
                    RisePropertyChanged("SessionsFilesDirectoryPath");
                    _isSettingsChanged = true;
                }
            }
        }


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
                    CanExecuteDelegate = x => _isValid && _isSettingsChanged
                });
            }
        }


        public string DBServerName
        {
            get { return _dbServerName; }
            set
            {
                if (value != _dbServerName)
                {
                    _dbServerName = value;
                    RisePropertyChanged("DBServerName");
                    _isSettingsChanged = true;
                }
            }
        }

        public string DBName
        {
            get { return _dbName; }
            set
            {
                if (value != _dbName)
                {
                    _dbName = value;
                    RisePropertyChanged("DBName");
                    _isSettingsChanged = true;
                }
            }
        }

        public string DBLogin
        {
            get { return _dbLogin; }
            set
            {
                if (value != _dbLogin)
                {
                    _dbLogin = value;
                    RisePropertyChanged("DBLogin");
                    _isSettingsChanged = true;
                }
            }
        }

        public string DBPassword
        {
            get { return _dbPassword; }
            set
            {
                if (value != _dbPassword)
                {
                    _dbPassword = value;
                    RisePropertyChanged("DBPassword");
                    _isSettingsChanged = true;
                }
            }
        }

        public SettingsViewModel(
            DataBaseRepository dataBaseRepository,
            ICardioSettings settings)
        {
            if (dataBaseRepository == null) throw new ArgumentNullException(nameof(dataBaseRepository));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _dataBaseRepository = dataBaseRepository;
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
            DBLogin = _settings.DataBase.User;
            DBName = _settings.DataBase.DataBase;
            DBPassword = _settings.DataBase.Password;
            DBServerName = _settings.DataBase.Source;
            _isValid = true;
            _isSettingsChanged = false;
        }

        #region IDataErrorInfoRealisation

        public string this[string columnName]
        {
            get
            {
                if (columnName == "SessionsFilesDirectoryPath" && !Directory.Exists(SessionsFilesDirectoryPath))
                {
                    _isValid = false;
                    return Localisation.SettingsViewModel_DirectoryDoesNotExist;
                }
                if (columnName == "DBServerName" || columnName == "DBName" || columnName == "DBLogin" || columnName == "DBPassword")
                {
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
            var message = String.Empty;
            try
            {
                await _dataBaseRepository.CheckConnectionAsync(DBName, DBServerName, DBLogin, DBPassword);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!String.IsNullOrEmpty(message))
            {
                await MessageHelper.Instance.ShowMessageAsync(message);
            }
            else
            {
                /* CardioSettings.Instance.SelectedAcentColorName = SelectedAccentColor.Name;
                CardioSettings.Instance.SeletedAppThemeName = SelectedAppTheme.Name;*/
                _settings.SessionsFilesDirectoryPath = SessionsFilesDirectoryPath;

                _settings.DataBase.User = DBLogin;
                _settings.DataBase.DataBase = DBName;
                _settings.DataBase.Password = DBPassword;
                _settings.DataBase.Source = DBServerName;

                //TODO saving settings
                //CardioMonitor.Settings.CardioSettings.SaveToFile();
                _isSettingsChanged = false;
                await MessageHelper.Instance.ShowMessageAsync(Localisation.SettingsViewModel_Saved);
                
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
    }
}
