using System;
using CardioMonitor.Devices.WpfModule;

namespace CardioMonitor.Devices
{
    public interface IDeviceModulesController
    {
        void Register(IWpfDeviceModule module);

        IDeviceControllerConfigViewModel GetViewModel(Guid deviceId);

        IDeviceControllerConfigBuilder GetConfigBuilder(Guid deviceId);
    }
}