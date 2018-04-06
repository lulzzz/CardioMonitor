using System;
using System.Windows;
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
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            //initialize messageHelper
            MessageHelper.Instance.Window = this;
            _viewModel = viewModel;
            HamburgerMenuControl.DataContext = _viewModel;
           // _viewModel.SessionViewModel.ThreadAssistant = new ThreadAssistant(this);
            DataContext = _viewModel;
            //SettingsView.ViewModel = _viewModel.SettingsViewModel;

            /*try
            {
                var accent = ThemeManager.GetAccent(Core.CardioSettings.CardioSettings.Instance.SelectedAcentColorName);
                var appTheme = ThemeManager.GetAppTheme(Core.CardioSettings.CardioSettings.Instance.SeletedAppThemeName);
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
            _viewModel.OpenStartStoryboard();
        }

        private async void MetroWindow_Closed(object sender, EventArgs e)
        {   
        }



        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*
            e.Cancel = true;
            var closingDialogresult = 
                await MessageHelper.Instance.ShowMessageAsync(
                    "Вы уверены, что хотите закрыть программу?", 
                    "Cardio Monitor", 
                    MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true);
            if (closingDialogresult == MessageDialogResult.Affirmative) 
            {
                //Необходимо для закрытие из асинхронности
                Closing -= MetroWindow_Closing;
                Close();
                e.Cancel = false;
            }*/
        }
        
    }
}
