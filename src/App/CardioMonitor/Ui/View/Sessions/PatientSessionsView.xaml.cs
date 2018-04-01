using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionsView.xaml
    /// </summary>
    public partial class PatientSessionsView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public PatientSessionsView()
        {

            InitializeComponent();
        }
    }
}
