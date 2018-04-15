using System;

namespace CardioMonitor.Devices.Bed.Infrastructure
{
    /// <summary>
    /// Параметры инициализации контроллера взаимодействия с инверсионным столом
    /// </summary>
    /// <remarks>
    /// Параметры могут отличаться в зависимости от способ подключения к устройству
    /// </remarks>
    public interface IBedControllerConfig : IDeviceControllerConfig
    {
        /// <summary>
        /// Максимальный угол кровати по оси Х, до которой она будет подниматься
        /// </summary>
        float MaxAngleX { get; }
        
        /// <summary>
        /// Количество циклов (повторений)
        /// </summary>
        short CyclesCount { get; }
        
        /// <summary>
        /// Частота движения
        /// </summary>
        float MovementFrequency { get; }
        
        /// <summary>
        /// Период опроса устройства
        /// </summary>
        TimeSpan UpdateDataPeriod { get; }
        
        /// <summary>
        /// Таймаут операций
        /// </summary>
        TimeSpan Timeout { get; }
    }
}