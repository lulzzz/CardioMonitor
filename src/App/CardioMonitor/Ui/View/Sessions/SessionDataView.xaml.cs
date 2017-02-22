using System.Windows.Controls;
using CardioMonitor.Ui.ViewModel.Sessions;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionDataView.xaml
    /// </summary>
    public partial class SessionDataView : UserControl
    {
        public SessionDataViewModel ViewModel
        {
            get { return DataContext as SessionDataViewModel; }
            set { DataContext = value; }
        }

        public SessionDataView()
        {
            InitializeComponent();
        }
    }
}
