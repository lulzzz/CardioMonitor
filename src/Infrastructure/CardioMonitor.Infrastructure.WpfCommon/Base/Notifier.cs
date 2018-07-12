using System.ComponentModel;
using System.Windows;

namespace CardioMonitor.Infrastructure.WpfCommon.Base
{
    public class Notifier : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
