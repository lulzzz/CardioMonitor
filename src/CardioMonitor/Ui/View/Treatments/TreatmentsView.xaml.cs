using System.Windows.Controls;
using CardioMonitor.Ui.ViewModel.Treatments;

namespace CardioMonitor.Ui.View.Treatments
{
    /// <summary>
    /// Interaction logic for TreatmentsView.xaml
    /// </summary>
    public partial class TreatmentsView : UserControl
    {
        private TreatmentsViewModel _viewModel;

        public TreatmentsView()
        {
            _viewModel = new TreatmentsViewModel();
            DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
