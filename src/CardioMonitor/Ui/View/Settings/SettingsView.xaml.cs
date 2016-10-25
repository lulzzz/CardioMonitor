using CardioMonitor.Ui.ViewModel.Settings;
using UserControl = System.Windows.Controls.UserControl;

namespace CardioMonitor.Ui.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
            set { DataContext = value; }
        }

        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
