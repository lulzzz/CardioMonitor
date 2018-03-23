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
        public IBedControllerInitParams BedControllerInitParams { get; }
        
        /// <summary>
        /// Параметры инициализации контроллера монитора
        /// </summary>
        public IMonitorControllerInitParams MonitorControllerInitParams { get; }
        
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
            IBedControllerInitParams bedControllerInitParams, 
            IMonitorControllerInitParams monitorControllerInitParams,
            short pumpingNumberOfAttemptsOnStartAndFinish, 
            short pumpingNumberOfAttemptsOnProcessing, 
            TimeSpan? deviceReconnectionTimeout = null)
        {
            CycleCount = cycleCount;
            UpdateDataPeriod = updateDataPeriod;
            BedControllerInitParams = bedControllerInitParams;
            MonitorControllerInitParams = monitorControllerInitParams;
            PumpingNumberOfAttemptsOnStartAndFinish = pumpingNumberOfAttemptsOnStartAndFinish;
            PumpingNumberOfAttemptsOnProcessing = pumpingNumberOfAttemptsOnProcessing;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
        }
    }
}