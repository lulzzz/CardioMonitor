using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.BLL.SessionProcessing
{
    /// <summary>
    /// Параметры сеанса
    /// </summary>
    public class SessionParams
    {
        /// <summary>
        /// Максимальный угол наклона кровати
        /// </summary>
        public float MaxAngle { get; }
        
        /// <summary>
        /// Количество повторений (циклов)
        /// </summary>
        public short CycleCount { get; }
        
        /// <summary>
        /// Частота
        /// </summary>
        public float Frequency { get; }
        
        /// <summary>
        /// Период обновления данных
        /// </summary>
        public TimeSpan UpdateDatePeriod { get; }
        
        /// <summary>
        /// Параметры инициализации контроллера кровати
        /// </summary>
        public IBedControllerInitParams BedControllerInitParams { get; }
        
        /// <summary>
        /// Параметры инициализации контроллера монитора
        /// </summary>
        public IMonitorControllerInitParams MonitorControllerInitParams { get; }

        public SessionParams(float maxAngle, short cycleCount, float frequency, TimeSpan updateDatePeriod, IBedControllerInitParams bedControllerInitParams, IMonitorControllerInitParams monitorControllerInitParams)
        {
            MaxAngle = maxAngle;
            CycleCount = cycleCount;
            Frequency = frequency;
            UpdateDatePeriod = updateDatePeriod;
            BedControllerInitParams = bedControllerInitParams;
            MonitorControllerInitParams = monitorControllerInitParams;
        }
    }
}