using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionProcessingInitView.xaml
    /// </summary>
    public partial class SessionProcessingInitView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }
        public SessionProcessingInitView()
        {
            InitializeComponent();
        }
    }
}
