using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using CardioMonitor.Core;
using CardioMonitor.Ui.Sessions;
using CardioMonitor.ViewModel;
using CardioMonitor.ViewModel.Sessions;
using OxyPlot;
using Binding = System.Windows.Forms.Binding;
using UserControl = System.Windows.Controls.UserControl;

namespace CardioMonitor.View
{
    /// <summary>
    /// Interaction logic for PatientTreatmentSession.xaml
    /// </summary>
    public partial class PatientTreatmentSession : UserControl
    {

        public SessionViewModel ViewModel
        {
            get { return DataContext as SessionViewModel; }
            set
            {
                DataContext = value;
            }
        }

        public PatientTreatmentSession()
        {
            InitializeComponent();
        }

        private void OnDataPointsChanged(object sender, EventArgs args)
        {
            EcgView.Update();
        }
    }
}
