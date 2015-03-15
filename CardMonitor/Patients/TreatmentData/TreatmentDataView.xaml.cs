using System.Windows.Controls;

namespace CardioMonitor.Patients.TreatmentData
{
    /// <summary>
    /// Interaction logic for TreatmentDataView.xaml
    /// </summary>
    public partial class TreatmentDataView : UserControl
    {
        public TreatmentDataViewModel Model
        {
            get { return DataContext as TreatmentDataViewModel; }
            set { DataContext = value; }
        }

        public TreatmentDataView()
        {
            Model = new TreatmentDataViewModel();

            InitializeComponent();
        }
    }
}
