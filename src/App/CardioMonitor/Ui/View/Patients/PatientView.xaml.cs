using System.Windows.Controls;
using CardioMonitor.Ui.ViewModel.Patients;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Patients
{
    /// <summary>
    /// Interaction logic for PatientView.xaml
    /// </summary>
    public partial class PatientView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public PatientView()
        {
            InitializeComponent();
        }
    }
}
