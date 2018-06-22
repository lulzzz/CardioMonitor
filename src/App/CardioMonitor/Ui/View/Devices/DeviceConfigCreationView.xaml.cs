using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Devices
{
    /// <summary>
    /// Interaction logic for DeviceConfigCreationView.xaml
    /// </summary>
    public partial class DeviceConfigCreationView : IStoryboardPageView
    {
        public DeviceConfigCreationView()
        {
            InitializeComponent();
        }

        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }
    }
}
