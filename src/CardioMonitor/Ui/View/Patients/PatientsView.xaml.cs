using System.Windows.Controls;
using CardioMonitor.Ui.ViewModel.Patients;
using UserControl = System.Windows.Controls.UserControl;

namespace CardioMonitor.Ui.View.Patients
{
    /// <summary>
    /// Interaction logic for PatientsView.xaml
    /// </summary>
    public partial class PatientsView : UserControl
    {
        private readonly PatientsViewModel _viewModel;

        public PatientsView()
        {
            _viewModel = new PatientsViewModel();
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void SearTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (0 == SearTB.Text.Length)
            {
                _viewModel.CancelSearch();
            }
        }


    }
}
