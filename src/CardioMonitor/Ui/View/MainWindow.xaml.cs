using System;
using System.Windows;
using CardioMonitor.Threading;
using CardioMonitor.Ui.ViewModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.Ui.View
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
            /*try
            {
                var accent = ThemeManager.GetAccent(Core.Settings.Settings.Instance.SelectedAcentColorName);
                var appTheme = ThemeManager.GetAppTheme(Core.Settings.Settings.Instance.SeletedAppThemeName);
                ThemeManager.ChangeAppStyle(Application.Current, accent, appTheme);
            }
            catch
            {

            }*/
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

        private async void MetroWindow_Closed(object sender, EventArgs e)
        {   
        }

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            var closingDialogresult = await MessageHelper.Instance.ShowMessageAsync("Вы уверены, что хотите закрыть программу?", "Cardio Monitor", MessageDialogStyle.AffirmativeAndNegative);
            if (closingDialogresult == MessageDialogResult.Affirmative) 
            {
                //Необходимо для закрытие из асинхронности
                Closing -= MetroWindow_Closing;
                Close();
                e.Cancel = false;
            }
        }

    }
}
