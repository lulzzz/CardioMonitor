using System.Windows.Controls;
using CardioMonitor.Ui.ViewModel.Sessions;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionsView.xaml
    /// </summary>
    public partial class PatientSessionsView : UserControl
    {
        private PatientSessionsViewModel _viewModel;

        public PatientSessionsView()
        {
            DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
