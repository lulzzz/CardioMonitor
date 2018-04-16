using System.ComponentModel;
using System.Runtime.CompilerServices;
using CardioMonitor.Devices.Bed.Fake.WpfModule.Annotations;

namespace CardioMonitor.Devices.Bed.Fake.WpfModule
{
    public class FadeDeviceControllerConfigViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string this[string columnName] => throw new System.NotImplementedException();

        public string Error { get; }
    }
}