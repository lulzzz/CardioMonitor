using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionsView.xaml
    /// </summary>
    public partial class SessionsView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public SessionsView()
        {
            InitializeComponent();
        }
    }
}
