using System;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionProcessingView.xaml
    /// </summary>
    public partial class SessionProcessingView : IStoryboardPageView
    {

        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }

        public SessionProcessingView()
        {
            InitializeComponent();
        }

        private void OnDataPointsChanged(object sender, EventArgs args)
        {
            //EcgView.Update();
        }
    }
}
