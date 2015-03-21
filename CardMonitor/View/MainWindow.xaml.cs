using System;
using System.Windows;
using CardioMonitor.Core;
using CardioMonitor.ViewModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            //inititalize messageHelper
            MessageHelper.Instance.Window = this;
            _viewModel = new MainWindowViewModel();
            _viewModel.SessionViewModel.ThreadAssistant = new ThreadAssistant(this);
            DataContext = _viewModel;
            InitializeComponent();
            SettingsView.ViewModel = _viewModel.SettingsViewModel;
            try
            {
                var accent = ThemeManager.GetAccent(Settings.Settings.Instance.SelectedAcentColorName);
                var appTheme = ThemeManager.GetAppTheme(Settings.Settings.Instance.SeletedAppThemeName);
                ThemeManager.ChangeAppStyle(Application.Current, accent, appTheme);
            }
            catch
            {

            }
        }

        private void SettingsB_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

                var flyout = this.Flyouts.Items[0] as Flyout;
                if (flyout == null)
                {
                    return;
                }
                if (!flyout.IsOpen)
                {
                  flyout.IsOpen =  true;
                }
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("Error", ex.Message);
            }

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdatePatiens();
        }

    }
}
