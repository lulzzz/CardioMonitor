﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class SessionProcessor :  INotifyPropertyChanged, IDisposable
    {
        [CanBeNull] 
        private SessionParams _startParams;


        /// <summary>
        /// Всегда устанавливается при инициализации
        /// </summary>
        [NotNull]
        private IDevicesFacade _devicesFacade;

        [NotNull]
        private IUiInvoker _uiInvoker;


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

        /// <summary>
        /// Текущий угол наклона кровати по оси X
        /// </summary>
        public float CurrentXAngle
        {
            get => _currentXAngle;
            set
            {
                if (Math.Abs(_currentXAngle - value) > CardioMonitorConstants.Tolerance)
                {
                    _currentXAngle = value;
                    RisePropertyChanged(nameof(CurrentXAngle));
                }
            }
        }
        private float _currentXAngle;

        /// <summary>
        /// Прошедшее время с начала сеанса
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set { _elapsedTime = value;
                RisePropertyChanged(nameof(ElapsedTime)); }
        }
        private TimeSpan _elapsedTime;

        /// <summary>
        /// Оставшееся время до конца сеанса
        /// </summary>
        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                _remainingTime = value; 
                RisePropertyChanged(nameof(RemainingTime));
            }
        }
        private TimeSpan _remainingTime;

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus SessionStatus
        {
            get => _sessionStatus;
            set
            {
                _sessionStatus = value;
                RisePropertyChanged(nameof(SessionStatus));
                OnSessionStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private SessionStatus _sessionStatus;

        #endregion

        #region Events

        public event EventHandler<SessionProcessingException> OnException;
        
        public event EventHandler<Exception> OnSessionErrorStop;
       
        public event EventHandler OnSessionCompleted;

        public event EventHandler<short> OnCycleCompleted;

        public event EventHandler OnPausedFromDevice;
        
        public event EventHandler OnResumedFromDevice;
        
        public event EventHandler OnEmeregencyStoppedFromDevice;
        
        public event EventHandler OnReversedFromDevice;

        public event EventHandler OnSessionStatusChanged;

        public event EventHandler<ReconnectionEventArgs> OnInversionTableReconnectionStarted;
        public event EventHandler<ReconnectionEventArgs> OnInversionTableReconnectionWaiting;
        public event EventHandler OnInversionTableReconnected;
        public event EventHandler OnInversionTableReconnectionFailed;
        
        public event EventHandler<ReconnectionEventArgs> OnMonitorReconnectionStarted;
        public event EventHandler<ReconnectionEventArgs> OnMonitorReconnectionWaiting;
        public event EventHandler OnMonitorReconnected;
        public event EventHandler OnMonitorReconnectionFailed;


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

            _devicesFacade.OnException += HandleGettingPatientParamsException;
            _devicesFacade.OnSessionErrorStop += HandleOnSessionErrorStop;
            _devicesFacade.OnCycleCompleted += HandleOnCycleCompleted;
            _devicesFacade.OnElapsedTimeChanged += HandleOnElapsedTimeChanged;
            _devicesFacade.OnRemainingTimeChanged += HandleOnRemainingTimeChanged;
            _devicesFacade.OnSessionCompleted += HandleOnSessionCompleted;
            _devicesFacade.OnPausedFromDevice += HandleOnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice += HandleOnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice += HandleOnEmeregencyStoppedFromDevice;
            _devicesFacade.OnReversedFromDevice += OnReversedFromDevice;
            _devicesFacade.OnCurrentAngleXRecieved += HandleOnCurrentAngleXRecieved;
            _devicesFacade.OnCommonPatientParamsRecieved += HandleOnCommonPatientParamsRecieved;
            _devicesFacade.OnPatientPressureParamsRecieved += HandleOnPatientPressureParamsRecieved;
            _devicesFacade.OnInversionTableReconnected += OnInversionTableReconnected;
            _devicesFacade.OnInversionTableReconnectionFailed += OnInversionTableReconnectionFailed;
            _devicesFacade.OnInversionTableReconnectionStarted += OnInversionTableReconnectionStarted;
            _devicesFacade.OnInversionTableReconnectionWaiting += OnInversionTableReconnectionWaiting;
            _devicesFacade.OnMonitorReconnected += OnMonitorReconnected;
            _devicesFacade.OnMonitorReconnectionFailed += OnMonitorReconnectionFailed;
            _devicesFacade.OnMonitorReconnectionStarted += OnMonitorReconnectionStarted;
            _devicesFacade.OnMonitorReconnectionWaiting += OnMonitorReconnectionWaiting;

            var temp = new List<CycleData>(startParams.CycleCount);
            for (var index = 0; index < startParams.CycleCount; index++)
            {
                temp.Add(new CycleData
                {
                    CycleNumber = (short) (index + 1),
                    CycleParams = new ObservableCollection<CheckPointParams>()
                });
            }

            _uiInvoker = uiInvoker ?? throw new ArgumentNullException(nameof(uiInvoker));
            _uiInvoker.Invoke(() =>
            {
                PatientParamsPerCycles = temp;
            });
            _isInitialized = true;
        }

        public async Task<bool> StartAsync()
        {
            AssureInitialization();

            var isSuccessfull = await _devicesFacade
                .StartAsync()
                .ConfigureAwait(false);
            if (isSuccessfull)
            {
                _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.InProgress; });
            }

            return isSuccessfull;
        }

        private void AssureInitialization()
        {
            if (!_isInitialized) throw new InvalidOperationException($"{nameof(SessionProcessor)} не инициализирован. Необходимо сначала вызвать метод {nameof(Init)}");
        }

        public async Task<bool> EmeregencyStopAsync()
        {
            AssureInitialization();

            var isSuccessfull = await _devicesFacade
                .EmergencyStopAsync()
                .ConfigureAwait(false);
            _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.EmergencyStopped; });

            return isSuccessfull;
        }

        public async Task<bool> PauseAsync()
        {
            AssureInitialization();
            var isSuccessfull = await _devicesFacade
                .PauseAsync()
                .ConfigureAwait(false);
            if (isSuccessfull)
            {
                _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.Suspended; });
            }

            return isSuccessfull;
        }

        public async Task<bool> ResumeAsync()
        {
            AssureInitialization();

            var isSuccessfull = await _devicesFacade
                .StartAsync()
                .ConfigureAwait(false);
            if (isSuccessfull)
            {
                _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.InProgress; });
            }

            return isSuccessfull;
        }

        public async Task<bool> ReverseAsync()
        {
            AssureInitialization();

            return await _devicesFacade
                .ProcessReverseRequestAsync()
                .ConfigureAwait(false);
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

        public async Task<bool> RequestManualDataUpdateAsync()
        {
            AssureInitialization();

            return await _devicesFacade
                .ForceDataCollectionRequestAsync()
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (_devicesFacade == null) return;
            
            _devicesFacade.Dispose();
            
            _devicesFacade.OnException -= HandleGettingPatientParamsException;
            _devicesFacade.OnSessionErrorStop -= HandleOnSessionErrorStop;
            _devicesFacade.OnCycleCompleted -= HandleOnCycleCompleted;
            _devicesFacade.OnElapsedTimeChanged -= HandleOnElapsedTimeChanged;
            _devicesFacade.OnRemainingTimeChanged -= HandleOnRemainingTimeChanged;
            _devicesFacade.OnSessionCompleted -= HandleOnSessionCompleted;
            _devicesFacade.OnPausedFromDevice -= HandleOnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice -= OnResumedFromDevice;
            _devicesFacade.OnResumedFromDevice -= HandleOnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice -= HandleOnEmeregencyStoppedFromDevice;
            _devicesFacade.OnCurrentAngleXRecieved -= HandleOnCurrentAngleXRecieved;
            _devicesFacade.OnCommonPatientParamsRecieved -= HandleOnCommonPatientParamsRecieved;
            _devicesFacade.OnPatientPressureParamsRecieved -= HandleOnPatientPressureParamsRecieved;
            
            _devicesFacade.OnInversionTableReconnected -= OnInversionTableReconnected;
            _devicesFacade.OnInversionTableReconnectionFailed -= OnInversionTableReconnectionFailed;
            _devicesFacade.OnInversionTableReconnectionStarted -= OnInversionTableReconnectionStarted;
            _devicesFacade.OnInversionTableReconnectionWaiting -= OnInversionTableReconnectionWaiting;
            _devicesFacade.OnMonitorReconnected -= OnMonitorReconnected;
            _devicesFacade.OnMonitorReconnectionFailed -= OnMonitorReconnectionFailed;
            _devicesFacade.OnMonitorReconnectionStarted -= OnMonitorReconnectionStarted;
            _devicesFacade.OnMonitorReconnectionWaiting -= OnMonitorReconnectionWaiting;
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
            OnCycleCompleted?.Invoke(sender, completedCycleNumber);
        }

        private void HandleGettingPatientParamsException(
            object sender, 
            [NotNull] SessionProcessingException exception)
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
            OnException?.Invoke(sender, exception);
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
                    if (cycleNumber == 0) return;
                    
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
                });
            }
        }

        private void HandleOnCurrentAngleXRecieved(object sender, float value)
        {
            _uiInvoker.Invoke(() => { CurrentXAngle = value; });
        }
        
        private void HandleOnRemainingTimeChanged(object sender, TimeSpan timeSpan)
        {
            _uiInvoker.Invoke(() => { RemainingTime = timeSpan; });
        }

        private void HandleOnElapsedTimeChanged(object sender, TimeSpan timeSpan)
        {
            _uiInvoker.Invoke(() => { ElapsedTime = timeSpan; });
        }
        
        private void HandleOnPausedFromDevice(object sender, EventArgs eventArgs)
        {
            _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.Suspended; });
            OnPausedFromDevice?.Invoke(sender, eventArgs);
        }

        private void HandleOnEmeregencyStoppedFromDevice(object sender, EventArgs eventArgs)
        {
            _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.EmergencyStopped; });
            OnEmeregencyStoppedFromDevice?.Invoke(sender, eventArgs);
        }

        private void HandleOnResumedFromDevice(object sender, EventArgs eventArgs)
        {
            _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.InProgress; });
            OnResumedFromDevice?.Invoke(sender, eventArgs);
        }

        private void HandleOnSessionCompleted(object sender, EventArgs eventArgs)
        {
            _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.Completed; });
            OnSessionCompleted?.Invoke(sender, eventArgs);
        }

        private void HandleOnSessionErrorStop(object sender, Exception exception)
        {
            _uiInvoker.Invoke(() => { SessionStatus = SessionStatus.TerminatedOnError; });
            OnSessionErrorStop?.Invoke(sender, exception);
        }

        #endregion


        #region Rise events 

        public event PropertyChangedEventHandler PropertyChanged;

        public void RisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}