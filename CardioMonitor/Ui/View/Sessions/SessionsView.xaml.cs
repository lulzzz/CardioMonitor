using System.Windows.Controls;
using CardioMonitor.Ui.ViewModel.Sessions;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionsView.xaml
    /// </summary>
    public partial class PatientTreatmentSessionsView : UserControl
    {
        private SessionsViewModel _viewModel;

        public PatientTreatmentSessionsView()
        {
            _viewModel = new SessionsViewModel();
            DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
