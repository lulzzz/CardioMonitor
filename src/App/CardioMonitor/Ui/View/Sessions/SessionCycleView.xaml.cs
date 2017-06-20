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
using CardioMonitor.Ui.ViewModel.Sessions;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionCycleView.xaml
    /// </summary>
    public partial class SessionCycleView : UserControl
    {
        public SessionCycleViewModel ViewModel
        {
            get {return DataContext as SessionCycleViewModel; }
            set { DataContext = value; }
        }

        public SessionCycleView()
        {
            InitializeComponent();
        }
    }
}
