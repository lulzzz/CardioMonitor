using System.ComponentModel;
using System.Windows;

namespace CardioMonitor.Ui.Base
{
    public class Notifier : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
