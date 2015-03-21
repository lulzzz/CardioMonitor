using CardioMonitor.ViewModel;
using UserControl = System.Windows.Controls.UserControl;

namespace CardioMonitor.View
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
