using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionDataView.xaml
    /// </summary>
    public partial class SessionDataView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public SessionDataView()
        {
            InitializeComponent();
        }
    }
}
