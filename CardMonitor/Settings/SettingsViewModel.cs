using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;

namespace CardioMonitor.Settings
{
    public class SettingsViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private AppApperanceData _selectedAccentColor;
        private AppApperanceData _selectedAppTheme;

        public List<AppApperanceData> AccentColors { get; set; }
        public List<AppApperanceData> AppThemes { get; set; }

        public SettingsViewModel()
        {
            AccentColors = ThemeManager.Accents
                                            .Select(a => new AppApperanceData { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                            .ToList();

            AppThemes = ThemeManager.AppThemes
                                           .Select(a => new AppApperanceData { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                                           .ToList();
            
        }

        #region INotifyRealisation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDataErrorInfoRealisation

        public string this[string columnName]
        {
            get { return null; }
        }

        public string Error { get { return string.Empty; } }

        #endregion

        public AppApperanceData SelectedAccentColor
        {
            get { return _selectedAccentColor; }
            set
            {
                if (_selectedAccentColor != value)
                {
                    _selectedAccentColor = value;
                    RaisePropertyChanged("SelectedAccentColor");

                    var theme = ThemeManager.DetectAppStyle(Application.Current);
                    var accent = ThemeManager.GetAccent(value.Name);
                    ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
                    Settings.Instance.SelectedAcentColorName = value.Name;
                    Settings.SaveToFile();
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
                    RaisePropertyChanged("SelectedAppTheme");

                    var theme = ThemeManager.DetectAppStyle(Application.Current);
                    var appTheme = ThemeManager.GetAppTheme(value.Name);
                    ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
                    Settings.Instance.SeletedAppThemeName = value.Name;
                    Settings.SaveToFile();
                }
            }
        }
    }
}
