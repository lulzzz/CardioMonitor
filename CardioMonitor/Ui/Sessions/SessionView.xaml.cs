using System.Windows.Controls;
using CardioMonitor.Core;
using CardioMonitor.ViewModel;
using CardioMonitor.ViewModel.Sessions;

namespace CardioMonitor.View
{
    /// <summary>
    /// Interaction logic for PatientTreatmentSession.xaml
    /// </summary>
    public partial class PatientTreatmentSession : UserControl
    {
        private SessionViewModel _viewModel;

        public PatientTreatmentSession()
        {
            _viewModel = new SessionViewModel ();
            DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
