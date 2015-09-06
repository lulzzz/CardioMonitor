using System.Windows.Controls;
using CardioMonitor.Ui.Sessions;

namespace CardioMonitor.View
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
