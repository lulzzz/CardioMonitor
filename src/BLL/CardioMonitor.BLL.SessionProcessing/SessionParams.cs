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
        /// Количество повторений (циклов)
        /// </summary>
        public short CycleCount { get; }
        
        /// <summary>
        /// Период обновления данных
        /// </summary>
        public TimeSpan UpdateDataPeriod { get; }
        
        /// <summary>
        /// Параметры инициализации контроллера кровати
        /// </summary>
        public IBedControllerConfig BedControllerConfig { get; }
        
        /// <summary>
        /// Параметры инициализации контроллера монитора
        /// </summary>
        public IMonitorControllerConfig MonitorControllerConfig { get; }
        
        /// <summary>
        /// Количество попыток накачки при старте
        /// </summary>
        public short PumpingNumberOfAttemptsOnStartAndFinish { get; }
        
        /// <summary>
        /// Количество попыток накачики в процессе выполнения сеанса
        /// </summary>
        public short PumpingNumberOfAttemptsOnProcessing { get; }

        /// <summary>
        /// Время, через которое будет осуществляться попытка переподключения к устройству
        /// </summary>
        /// <remarks>
        /// Если null, то переподключения не будет
        /// </remarks>
        public TimeSpan? DeviceReconnectionTimeout { get; }
        
        public SessionParams(
            short cycleCount, 
            TimeSpan updateDataPeriod, 
            IBedControllerConfig bedControllerConfig, 
            IMonitorControllerConfig monitorControllerConfig,
            short pumpingNumberOfAttemptsOnStartAndFinish, 
            short pumpingNumberOfAttemptsOnProcessing, 
            TimeSpan? deviceReconnectionTimeout = null)
        {
            CycleCount = cycleCount;
            UpdateDataPeriod = updateDataPeriod;
            BedControllerConfig = bedControllerConfig;
            MonitorControllerConfig = monitorControllerConfig;
            PumpingNumberOfAttemptsOnStartAndFinish = pumpingNumberOfAttemptsOnStartAndFinish;
            PumpingNumberOfAttemptsOnProcessing = pumpingNumberOfAttemptsOnProcessing;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
        }
    }
}