using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Devices
{
    /// <summary>
    /// Interaction logic for DeviceConfigsView.xaml
    /// </summary>
    public partial class DeviceConfigsView : IStoryboardPageView
    {
        public DeviceConfigsView()
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
