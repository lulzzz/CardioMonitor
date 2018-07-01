using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
using PatientPressureParams = CardioMonitor.BLL.SessionProcessing.DeviceFacade.PatientPressureParams;

namespace CardioMonitor.BLL.SessionProcessing
{
    /// <summary>
    /// Класс для обработки сеанаса: данных, команды управления
    /// </summary>
    /// <remarks>
    /// По факту высокоуровнеая обертка над всем процессом получения данных. Аргегируют данные в удобный для конечного потребителя вид.
    /// Сделан для того, чтобы потом могли легко портироваться на другой UI, чтобы не было жесткой завязки на WPF
    /// </remarks>
    public class SessionProcessor :  IDisposable
    {
        [CanBeNull] 
        private SessionParams _startParams;


        /// <summary>
        /// Всегда устанавливается при инициализации
        /// </summary>
        [NotNull]
        private IDevicesFacade _devicesFacade;
        

        [NotNull]
        private readonly object _cycleDataLocker = new object();

        private bool _isInitialized;

        public SessionProcessor()
        {
            CurrentCycleNumber = 0;
            SessionStatus = SessionStatus.NotStarted;
            _isInitialized = false;
        }

        #region Properties
        
        /// <summary>
        /// Показатели пациента с разделением по циклам
        /// </summary>
        /// <remarks>
        /// Обновляются в порядке поступления данных от устройства
        /// </remarks>
        public IReadOnlyList<CycleData> PatientParamsPerCycles
        {
            get => _patientParamsPerCycles;
            set
            {
                _patientParamsPerCycles = value;
                RisePropertyChanged(nameof(PatientParamsPerCycles));
            }
        }

        private IReadOnlyList<CycleData> _patientParamsPerCycles;
        /// <summary>
        /// Текущий номер цикла
        /// </summary>
        public short CurrentCycleNumber
        {
            get => _currentCycleNumber;
            set
            {
                var oldValue = _currentCycleNumber;
                _currentCycleNumber = value;
                if (oldValue != _currentCycleNumber)
                {
                    RisePropertyChanged(nameof(CurrentCycleNumber));
                }
            }
        }
        private short _currentCycleNumber;
        

        public SessionStatus SessionStatus
        {
            get => _sessionStatus;
            set
            {
                var oldValue = _sessionStatus;
                _sessionStatus = value;
                if (_sessionStatus != oldValue)
                {
                    SessionStatusChanged?.Invoke(this, value);
                }
            }
        }

        private SessionStatus _sessionStatus;

        #endregion

        #region Events

        public event EventHandler<SessionProcessingException> ExceptionOccured;
        
        public event EventHandler<Exception> SessionOnErrorStoped;
       
        public event EventHandler SessionCompleted;

        public event EventHandler<short> CycleCompleted;

        public event EventHandler PausedFromDevice;
        
        public event EventHandler ResumedFromDevice;
        
        public event EventHandler EmeregencyStoppedFromDevice;
        
        public event EventHandler ReversedFromDevice;

        public event EventHandler<SessionStatus> SessionStatusChanged;

        public event EventHandler<TimeSpan> ElapsedTimeChanged;

        public event EventHandler<TimeSpan> RemainingTimeChanged;

        public event EventHandler<float> CurrentAgnleXChanged;


        #endregion
        
        #region public methods

        public void Init(
            [NotNull] SessionParams startParams,
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController,
            [NotNull] IWorkerController workerController,
            [NotNull] ILogger logger,
            [NotNull] IUiInvoker uiInvoker)
        {
            if (bedController == null) throw new ArgumentNullException(nameof(bedController));
            if (monitorController == null) throw new ArgumentNullException(nameof(monitorController));
            if (workerController == null) throw new ArgumentNullException(nameof(workerController));
            _startParams = startParams ?? throw new ArgumentNullException(nameof(startParams));

            _devicesFacade = new DevicesFacade(
                startParams,
                bedController,
                monitorController,
                workerController,
                logger);

            _devicesFacade.OnException += ExceptionOccured;
            _devicesFacade.OnException += HandleOnException;
            _devicesFacade.OnSessionErrorStop += SessionOnErrorStoped;
            _devicesFacade.OnSessionErrorStop += HandleOnSessionErrorStop;
            _devicesFacade.OnCycleCompleted += HandleOnCycleCompleted;
            _devicesFacade.OnCycleCompleted += CycleCompleted;
            _devicesFacade.OnElapsedTimeChanged += HandleOnElapsedTimeChanged;
            _devicesFacade.OnRemainingTimeChanged += HandleOnRemainingTimeChanged;
            _devicesFacade.OnSessionCompleted += SessionCompleted;
            _devicesFacade.OnSessionCompleted += HandleOnSessionCompleted;
            _devicesFacade.OnPausedFromDevice += PausedFromDevice;
            _devicesFacade.OnPausedFromDevice += HandleOnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice += ResumedFromDevice;
            _devicesFacade.OnResumedFromDevice += HandleOnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice += EmeregencyStoppedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice += HandleOnEmeregencyStoppedFromDevice;
            _devicesFacade.OnReversedFromDevice += ReversedFromDevice;
            _devicesFacade.OnCurrentAngleXRecieved += HandleOnCurrentAngleXRecieved;
            _devicesFacade.OnCommonPatientParamsRecieved += HandleOnCommonPatientParamsRecieved;
            _devicesFacade.OnPatientPressureParamsRecieved += HandleOnPatientPressureParamsRecieved;

            var temp = new List<CycleData>(startParams.CycleCount);
            for (var index = 0; index < startParams.CycleCount; index++)
            {
                temp.Add(new CycleData((short) (index + 1)));
            }
            
            _uiInvoker.Invoke(() =>
            {
                PatientParamsPerCycles = temp;
            });
            _isInitialized = true;
        }

        public async Task StartAsync()
        {
            AssureInitialization();

            await _devicesFacade
                .StartAsync()
                .ConfigureAwait(true);

            SessionStatus = SessionStatus.InProgress;
        }

        private void AssureInitialization()
        {
            if (!_isInitialized) throw new InvalidOperationException($"{nameof(SessionProcessor)} не инициализирован. Необходимо сначала вызвать метод {nameof(Init)}");
        }

        public Task EmeregencyStopAsync()
        {
            AssureInitialization();


            SessionStatus = SessionStatus.EmergencyStopped;
            return _devicesFacade.EmergencyStopAsync();
        }

        public Task PauseAsync()
        {
            AssureInitialization();


            SessionStatus = SessionStatus.Suspended;
            return _devicesFacade.PauseAsync();
        }

        public Task ResumeAsync()
        {
            AssureInitialization();


            SessionStatus = SessionStatus.InProgress;
            return _devicesFacade.StartAsync();
        }

        public Task ReverseAsync()
        {
            AssureInitialization();
            
            return _devicesFacade.ProcessReverseRequestAsync();
        }

        public void EnableAutoPumping()
        {
            AssureInitialization();
            
            _devicesFacade.EnableAutoPumping();
        }

        public void DisableAutoPumping()
        {
            AssureInitialization();
            
            _devicesFacade.DisableAutoPumping();
        }

        public Task RequestManualDataUpdateAsync()
        {
            AssureInitialization();

            return _devicesFacade.ForceDataCollectionRequestAsync();
        }

        public void Dispose()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (_devicesFacade == null) return;
            
            _devicesFacade.OnException -= ExceptionOccured;
            _devicesFacade.OnException -= HandleOnException;
            _devicesFacade.OnSessionErrorStop -= SessionOnErrorStoped;
            _devicesFacade.OnSessionErrorStop -= HandleOnSessionErrorStop;
            _devicesFacade.OnCycleCompleted -= HandleOnCycleCompleted;
            _devicesFacade.OnElapsedTimeChanged -= HandleOnElapsedTimeChanged;
            _devicesFacade.OnRemainingTimeChanged -= HandleOnRemainingTimeChanged;
            _devicesFacade.OnSessionCompleted -= SessionCompleted;
            _devicesFacade.OnSessionCompleted -= HandleOnSessionCompleted;
            _devicesFacade.OnPausedFromDevice -= PausedFromDevice;
            _devicesFacade.OnPausedFromDevice -= HandleOnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice -= ResumedFromDevice;
            _devicesFacade.OnResumedFromDevice -= HandleOnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice -= EmeregencyStoppedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice -= HandleOnEmeregencyStoppedFromDevice;
            _devicesFacade.OnReversedFromDevice -= ReversedFromDevice;
            _devicesFacade.OnCurrentAngleXRecieved -= HandleOnCurrentAngleXRecieved;
            _devicesFacade.OnCommonPatientParamsRecieved -= HandleOnCommonPatientParamsRecieved;
            _devicesFacade.OnPatientPressureParamsRecieved -= HandleOnPatientPressureParamsRecieved;
        }

        #endregion

        #region private methods

        private void HandleOnCycleCompleted(object sender, short completedCycleNumber)
        {
            // это поле задается в методе Init
            if (_startParams == null) throw new InvalidOperationException($"Необходимо сначала выполнить {nameof(Init)}");


            _uiInvoker.Invoke(() =>
            {
                CurrentCycleNumber = completedCycleNumber == _startParams.CycleCount
                    ? completedCycleNumber
                    : (short) (completedCycleNumber + 1);
            });
        }

        private void HandleOnException(object sender, [NotNull] SessionProcessingException exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            if (PatientParamsPerCycles == null) throw new InvalidOperationException($"Необходимо сначала выполнить {nameof(Init)}");
            
            switch (exception.ErrorCode)
            {
                case SessionProcessingErrorCodes.PatientCommonParamsRequestError:
                case SessionProcessingErrorCodes.PatientCommonParamsRequestTimeout:
                {
                    var cycleNumber = exception.CycleNumber;
                    var iterationNumber = exception.IterationNumber;
                    if (iterationNumber == null || cycleNumber == null) return;
                    if (cycleNumber.Value != 0)
                    {
                        var checkPoint = PatientParamsPerCycles[cycleNumber.Value - 1]
                            .CycleParams.FirstOrDefault(x => x.IterationNumber == iterationNumber.Value);
                        if (checkPoint == null) return;
                        checkPoint.HandleErrorOnCommoParamsProcessing();
                    }

                    break;
                }
                case SessionProcessingErrorCodes.PatientPressureParamsRequestError:
                case SessionProcessingErrorCodes.PatientPressureParamsRequestTimeout:
                {
                    var cycleNumber = exception.CycleNumber;
                    var iterationNumber = exception.IterationNumber;
                    if (iterationNumber == null || cycleNumber == null) return;

                    if (cycleNumber.Value != 0)
                    {
                        var checkPoint = PatientParamsPerCycles[cycleNumber.Value - 1]
                            .CycleParams.FirstOrDefault(x => x.IterationNumber == iterationNumber.Value);
                        if (checkPoint == null) return;
                        checkPoint.HandleErrorOnPressureParamsProcessing();
                    }

                    break;
                }
            }
        }

        private void HandleOnPatientPressureParamsRecieved(
            object sender,
            PatientPressureParams patientPressureParams)
        {
            lock (_cycleDataLocker)
            {

                _uiInvoker.Invoke(() =>
                {
                    var cycleNumber = patientPressureParams.CycleNumber;
                    var iterationNumber = patientPressureParams.IterationNumber;
                    var inclinationAngle = patientPressureParams.InclinationAngle;
                    if (cycleNumber != 0)
                    {
                        var checkPoint = PatientParamsPerCycles[cycleNumber - 1].CycleParams
                            .FirstOrDefault(x => x.IterationNumber == iterationNumber);
                        if (checkPoint == null)
                        {
                            checkPoint = new CheckPointParams(
                                cycleNumber,
                                iterationNumber,
                                inclinationAngle);
                            PatientParamsPerCycles[cycleNumber - 1].CycleParams.Add(checkPoint);
                        }

                        checkPoint.SetPressureParams(patientPressureParams);
                    }
                });
            }
        }

        private void HandleOnCommonPatientParamsRecieved(
            object sender, 
            CommonPatientParams commonPatientParams)
        {
            lock (_cycleDataLocker)
            {
                _uiInvoker.Invoke(() =>
                {
                    var cycleNumber = commonPatientParams.CycleNumber;
                    var iterationNumber = commonPatientParams.IterationNumber;

                    var inclinationAngle = commonPatientParams.InclinationAngle;
                    if (cycleNumber != 0)
                    {
                        var checkPoint = PatientParamsPerCycles[cycleNumber - 1].CycleParams
                            .FirstOrDefault(x => x.IterationNumber == iterationNumber);
                        if (checkPoint == null)
                        {
                            checkPoint = new CheckPointParams(
                                cycleNumber,
                                iterationNumber,
                                inclinationAngle);
                            PatientParamsPerCycles[cycleNumber - 1].CycleParams.Add(checkPoint);
                        }

                        checkPoint.SetCommonParams(commonPatientParams);
                    }
                });
            }
        }

        private void HandleOnCurrentAngleXRecieved(object sender, float value)
        {
            CurrentAgnleXChanged?.Invoke(this, value);
        }
        
        private void HandleOnRemainingTimeChanged(object sender, TimeSpan value)
        {
            RemainingTimeChanged?.Invoke(this, value);
        }

        private void HandleOnElapsedTimeChanged(object sender, TimeSpan value)
        {
            ElapsedTimeChanged?.Invoke(this, value);
        }
        
        private void HandleOnPausedFromDevice(object sender, EventArgs eventArgs)
        {
            SessionStatus = SessionStatus.Suspended;
        }

        private void HandleOnEmeregencyStoppedFromDevice(object sender, EventArgs eventArgs)
        {
            SessionStatus = SessionStatus.EmergencyStopped;
        }

        private void HandleOnResumedFromDevice(object sender, EventArgs eventArgs)
        {
            SessionStatus = SessionStatus.InProgress;
        }

        private void HandleOnSessionCompleted(object sender, EventArgs eventArgs)
        {
            SessionStatus = SessionStatus.Completed;
        }

        private void HandleOnSessionErrorStop(object sender, Exception exception)
        {
             SessionStatus = SessionStatus.TerminatedOnError;
        }

        #endregion
    }
}