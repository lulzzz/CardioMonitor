﻿using System;
using CardioMonitor.Ui.ViewModel.Sessions;
using UserControl = System.Windows.Controls.UserControl;

namespace CardioMonitor.Ui.View.Sessions
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
            //EcgView.Update();
        }
    }
}