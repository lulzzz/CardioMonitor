using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardioMonitor.Patients.Treatments
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
