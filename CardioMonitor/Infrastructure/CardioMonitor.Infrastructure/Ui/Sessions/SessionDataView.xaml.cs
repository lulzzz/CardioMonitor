using System.Windows.Controls;

namespace CardioMonitor.Infrastructure.Ui.Sessions
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
