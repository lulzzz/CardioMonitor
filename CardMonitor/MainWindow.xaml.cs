using CardioMonitor.Core;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;

namespace CardioMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            //inititalize messageHelper
            MessageHelper.Instance.Window = this;
            DataContext = _viewModel;
            InitializeComponent();
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
                flyout.IsOpen = !flyout.IsOpen;
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("Error", ex.Message);
            }

        }

    }
}
