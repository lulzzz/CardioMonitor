using System.Windows.Controls;
using CardioMonitor.Ui.Patients;

namespace CardioMonitor.View
{
    /// <summary>
    /// Interaction logic for PatientView.xaml
    /// </summary>
    public partial class PatientView : UserControl
    {
        private readonly PatientViewModel _viewModel;

        public PatientView()
        {
            _viewModel = new PatientViewModel();
            DataContext = _viewModel;
            InitializeComponent();
        }
    }
}
