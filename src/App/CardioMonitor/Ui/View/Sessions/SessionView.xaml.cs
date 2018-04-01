using System;
using CardioMonitor.Ui.ViewModel.Sessions;
using UserControl = System.Windows.Controls.UserControl;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionProcessingView.xaml
    /// </summary>
    public partial class SessionProcessingView : UserControl
    {

        public SessionProcessingViewModel ViewModel
        {
            get { return DataContext as SessionProcessingViewModel; }
            set
            {
                DataContext = value;
            }
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
