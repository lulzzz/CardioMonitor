using System.Windows.Controls;
using CardioMonitor.ViewModel.Sessions;

namespace CardioMonitor.View
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
