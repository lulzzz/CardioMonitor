using System.Windows.Controls;
using CardioMonitor.ViewModel.Treatments;

namespace CardioMonitor.View
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
