using System;
using System.Windows;
using CardioMonitor.Ui.ViewModel;

namespace CardioMonitor.Ui.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            MessageHelper.Instance.Window = this;
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            HamburgerMenuControl.DataContext = _viewModel;
            DataContext = _viewModel;

            // applying theme
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

        

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenStartStoryboard();
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
