using System;

namespace CardioMonitor.Devices.WpfModule
{
    public interface IDeviceControllerConfigViewModel
    {
        bool CanGetConfig { get; }

        string GetConfigJson();

        void SetConfigJson(string jsonConfig);

        event EventHandler OnDataChanged;

        bool IsDataChanged { get; }

        void ResetDataChanges();
    }
}