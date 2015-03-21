using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Core;

namespace CardioMonitor.ViewModel
{
    public class SettingsViewModel : Notifier, IDataErrorInfo
    {
        private string _filesDirectoryPath;
        private ICommand _chooseFolderCommand;
        private ICommand _closeCommand;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;
        private bool _isValid;
        private bool _isSettingsChanged;

        #region Apperance settings
        /*
        private AppApperanceData _selectedAccentColor;
        private AppApperanceData _selectedAppTheme;
        public List<AppApperanceData> AccentColors { get; set; }
        public List<AppApperanceData> AppThemes { get; set; }

        public AppApperanceData SelectedAccentColor
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

        public AppApperanceData SelectedAppTheme
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

        public string FilesDirectoryPath
        {
            get { return _filesDirectoryPath; }
            set
            {
                if (value != _filesDirectoryPath)
                {
                    _filesDirectoryPath = value;
                    RisePropertyChanged("FilesDirectoryPath");
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

        public SettingsViewModel()
        {
            /*
            AccentColors = ThemeManager.Accents
                                            .Select(a => new AppApperanceData { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                            .ToList();

            AppThemes = ThemeManager.AppThemes
                                           .Select(a => new AppApperanceData { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                                           .ToList();*/
            InitializeSettings();
            _isSettingsChanged = false;
        }

        private void InitializeSettings()
        {
            /*SelectedAppTheme =
               AppThemes.FirstOrDefault(x => x.Name == Settings.Instance.SeletedAppThemeName);
            SelectedAccentColor =
                AccentColors.FirstOrDefault(x => x.Name == Settings.Instance.SelectedAcentColorName);*/
            FilesDirectoryPath = Settings.Settings.Instance.FilesDirectoryPath;
            _isValid = true;
            _isSettingsChanged = false;
        }

        #region IDataErrorInfoRealisation

        public string this[string columnName]
        {
            get
            {
                if (columnName == "FilesDirectoryPath" && !Directory.Exists(FilesDirectoryPath))
                {
                    _isValid = false;
                    return "Указанной директории не существует.";
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
                SelectedPath = FilesDirectoryPath,
                Description = "Выберите каталог для хранения результатов сеансов"
            };
            folderBrowserDialog.ShowDialog();
            FilesDirectoryPath = folderBrowserDialog.SelectedPath;
        }

        private async void SaveSettings()
        {
            await MessageHelper.Instance.ShowMessageAsync("Настройки сохранены");
           /* Settings.Instance.SelectedAcentColorName = SelectedAccentColor.Name;
            Settings.Instance.SeletedAppThemeName = SelectedAppTheme.Name;*/
            Settings.Settings.Instance.FilesDirectoryPath = FilesDirectoryPath;
            Settings.Settings.SaveToFile();
            _isSettingsChanged = false;
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
            Settings.Settings.LoadFromFile();
            InitializeSettings();
        }
    }
}
