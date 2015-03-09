using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardioMonitor.Patients.Sessions
{
    /// <summary>
    /// Interaction logic for SessionsView.xaml
    /// </summary>
    public partial class PatientTreatmentSessionsView : UserControl
    {
        private SessionsViewModel _viewModel;

        public PatientTreatmentSessionsView()
        {
            _viewModel = new SessionsViewModel();
            DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
