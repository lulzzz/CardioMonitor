using System;

namespace CardioMonitor.Devices
{
    /// <summary>
    /// Контроллер для взаимодействия с внешним устройством в рамках ПАК (программно-аппаратного комплекса)
    /// </summary>
    public interface IDeviceController : IDisposable
    {
        Guid DeviceId { get; }
        Guid DeviceTypeId { get; }
    }
}