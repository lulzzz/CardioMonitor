using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : IStoryboardPageView
    {
        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
