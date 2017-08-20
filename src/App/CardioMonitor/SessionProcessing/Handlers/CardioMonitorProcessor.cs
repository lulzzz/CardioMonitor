using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.SessionProcessing.Events;
using CardioMonitor.SessionProcessing.Events.Devices;
using Enexure.MicroBus;
using JetBrains.Annotations;

namespace CardioMonitor.SessionProcessing.Handlers
{
    /// <summary>
    /// Менеджер взаимодействия с кроватью
    /// </summary>
    public class CardioMonitorProcessor : 
        IEventHandler<PatientParamsRequestEvent>,
        IEventHandler<EcqRequestEvent>,
        IEventHandler<PumpingRequestedEvent>
    {

        /// <summary>
        /// Точность для сравнение double величин
        /// </summary>
        private const double Tolerance = 0.1e-12;

        /// <summary>
        /// Таймаут операции накачки
        /// </summary>
        private readonly TimeSpan _pumpingTimeout;

        /// <summary>
        /// Таймаут запроса параметром пациента
        /// </summary>
        private readonly TimeSpan _updatePatientParamTimeout;

        private readonly IMonitorController _monitorController;
        private readonly TaskHelper _taskHelper;

        private readonly IMicroBus _bus;

        public CardioMonitorProcessor(
            [NotNull] IMonitorController monitorController, 
            [NotNull] TaskHelper taskHelper,
            [NotNull] IMicroBus bus)
        {
            if (monitorController == null) throw new ArgumentNullException(nameof(monitorController));
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));
            if (bus == null) throw new ArgumentNullException(nameof(bus));

            _monitorController = monitorController;
            _taskHelper = taskHelper;
            _bus = bus;
            
            _updatePatientParamTimeout = new TimeSpan(0, 0, 8);
            _pumpingTimeout = new TimeSpan(0, 0, 8);
        }

        public async Task Handle([NotNull] PatientParamsRequestEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            PatientParams param;
            try
            {
                var gettingParamsTask = _monitorController.GetPatientParamsAsync();
                param = await _taskHelper.StartWithTimeout(gettingParamsTask, _updatePatientParamTimeout);
            }
            catch (TimeoutException)
            {
                param = new PatientParams
                {
                    RepsirationRate = -1,
                    HeartRate = -1
                };
            }
            param.InclinationAngle = Math.Abs(@event.CurrentAngle) < Tolerance ? 0 : @event.CurrentAngle;

            await _bus.PublishAsync(new PatientParamsRecievedEvent
            {
                Params = param
            });
        }

        public async Task Handle([NotNull] EcqRequestEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            //throw new System.NotImplementedException();
            await Task.Yield();
        }

        public async Task Handle([NotNull] PumpingRequestedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            bool isSuccessfully;
            try
            {
                var pumpingTask = _monitorController.PumpCuffAsync();
                isSuccessfully = await _taskHelper.StartWithTimeout(pumpingTask, _pumpingTimeout);
                
            }
            catch (TimeoutException)
            {
                isSuccessfully = false;
                //если бесконечено соединение
            }
            catch (Exception)
            {
                isSuccessfully = false;
                //на остальные случаи
                //TODO Вообще сюда надо будет добавить обработчики и логирование
            }

            await _bus.PublishAsync(new PumpingCompletedEvent {IsSuccessfully = isSuccessfully});
        }
    }
}