using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CardioMonitor.Core;
using UserControl = System.Windows.Controls.UserControl;

namespace CardioMonitor.Patients
{
    /// <summary>
    /// Interaction logic for PatientsView.xaml
    /// </summary>
    public partial class PatientsView : UserControl
    {
        private readonly PatientsViewModel _viewModel;

        public PatientsView()
        {
            _viewModel = new PatientsViewModel();
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void SearTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (0 == SearTB.Text.Length)
            {
                _viewModel.CancelSearch();
            }
        }


    }
}
