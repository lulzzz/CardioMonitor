using System.Windows.Controls;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Patients
{
    /// <summary>
    /// Interaction logic for PatientsView.xaml
    /// </summary>
    public partial class PatientsView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public PatientsView()
        {

            InitializeComponent();
        }

        private void SearTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (0 == SearTB.Text.Length)
            {
               // _viewModel.CancelSearch();
            }
        }


    }
}
