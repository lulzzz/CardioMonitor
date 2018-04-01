using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionCycleView.xaml
    /// </summary>
    public partial class SessionCycleView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public SessionCycleView()
        {
            InitializeComponent();
        }
    }
}
