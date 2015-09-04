using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using OxyPlot;

namespace CardioMonitor.Ui.EcgController
{
    /// <summary>
    /// Логика взаимодействия для EcgController.xaml
    /// </summary>
    public partial class EcgControllerView : UserControl
    {
        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
            "Points", typeof(ObservableCollection<DataPoint>), typeof(EcgControllerView), new PropertyMetadata(default(ObservableCollection<DataPoint>)));

        public ObservableCollection<DataPoint> Points
        {
            get { return (ObservableCollection<DataPoint>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public static readonly DependencyProperty NeedUpdateProperty = DependencyProperty.Register(
           "NeedUpdate", typeof(bool), typeof(EcgControllerView), new PropertyMetadata(default(bool), Update));

        public bool NeedUpdate
        {
            get { return (bool)GetValue(NeedUpdateProperty); }
            set { SetValue(NeedUpdateProperty, value); }
        }



        public EcgControllerView()
        {
            
            InitializeComponent();
        }

        private static void Update(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var ecgControllerView = sender as EcgControllerView;
            if (ecgControllerView == null)
            {
                return;
            }

            bool oldValue, newValue;
            if (!Boolean.TryParse(args.OldValue.ToString(), out oldValue))
            {
                return;
            }
            if (!Boolean.TryParse(args.NewValue.ToString(), out newValue))
            {
                return;
            }

            if (newValue & !oldValue)
            {
                ecgControllerView.Update();
                ecgControllerView.NeedUpdate = false;
            }
        }

        public void Update()
        {
            PlotView.InvalidatePlot();
        }
    }
}
