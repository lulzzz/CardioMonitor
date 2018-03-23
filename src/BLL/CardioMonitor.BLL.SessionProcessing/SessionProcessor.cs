using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;
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

        [CanBeNull]
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
        public IReadOnlyList<ObservableCollection<CheckPointParams>> PatientParamsPerCycles
        {
            get => _patientParamsPerCycles;
            set
            {
                _patientParamsPerCycles = value;
                RisePropertyChanged(nameof(PatientParamsPerCycles));
            }
        }

        private IReadOnlyList<ObservableCollection<CheckPointParams>> _patientParamsPerCycles;
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
            }
        }

        private SessionStatus _sessionStatus;

        #endregion

        #region Events

        public event EventHandler<SessionProcessingException> OnException;
        
        public event EventHandler<Exception> OnSessionErrorStop;
       
        public event EventHandler OnSessionCompleted;
        
        public event EventHandler OnPausedFromDevice;
        
        public event EventHandler OnResumedFromDevice;
        
        public event EventHandler OnEmeregencyStoppedFromDevice;
        
        public event EventHandler OnReversedFromDevice;


        #endregion
        
        #region Rise events 

        public event PropertyChangedEventHandler PropertyChanged;

        public void RisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region public methods

        public void Init(
            [NotNull] SessionParams startParams,
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController,
            [NotNull] IWorkerController workerController)
        {
            if (bedController == null) throw new ArgumentNullException(nameof(bedController));
            if (monitorController == null) throw new ArgumentNullException(nameof(monitorController));
            if (workerController == null) throw new ArgumentNullException(nameof(workerController));
            
            _startParams = startParams ?? throw new ArgumentNullException(nameof(startParams));
            
            _devicesFacade = new DevicesFacade(
                startParams,
                bedController,
                monitorController,
                workerController);
            
            _devicesFacade.OnException += OnException;
            _devicesFacade.OnException += HandleOnException;
            _devicesFacade.OnSessionErrorStop += OnSessionErrorStop;
            _devicesFacade.OnSessionErrorStop += HandleOnSessionErrorStop;
            _devicesFacade.OnCycleCompleted += HandleOnCycleCompleted;
            _devicesFacade.OnElapsedTimeChanged += HandleOnElapsedTimeChanged;
            _devicesFacade.OnRemainingTimeChanged += HandleOnRemainingTimeChanged;
            _devicesFacade.OnSessionCompleted += OnSessionCompleted;
            _devicesFacade.OnSessionCompleted += HandleOnSessionCompleted;
            _devicesFacade.OnPausedFromDevice += OnPausedFromDevice;
            _devicesFacade.OnPausedFromDevice += HandleOnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice += OnResumedFromDevice;
            _devicesFacade.OnResumedFromDevice += HandleOnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice += OnEmeregencyStoppedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice += HandleOnEmeregencyStoppedFromDevice;
            _devicesFacade.OnReversedFromDevice += OnReversedFromDevice;
            _devicesFacade.OnCurrentAngleXRecieved += HandleOnCurrentAngleXRecieved;
            _devicesFacade.OnCommonPatientParamsRecieved += HandleOnCommonPatientParamsRecieved;
            _devicesFacade.OnPatientPressureParamsRecieved += HandleOnPatientPressureParamsRecieved;
            
            PatientParamsPerCycles = new ObservableCollection<CheckPointParams>[startParams.CycleCount];
            _isInitialized = true;
        }
        
        public Task StartAsync()
        {
            AssureInitialization();
            
            SessionStatus = SessionStatus.InProgress;
            return _devicesFacade.StartAsync();
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

        public void Dispose()
        {
            if (_devicesFacade == null) return;
            
            _devicesFacade.OnException -= OnException;
            _devicesFacade.OnException -= HandleOnException;
            _devicesFacade.OnSessionErrorStop -= OnSessionErrorStop;
            _devicesFacade.OnSessionErrorStop -= HandleOnSessionErrorStop;
            _devicesFacade.OnCycleCompleted -= HandleOnCycleCompleted;
            _devicesFacade.OnElapsedTimeChanged -= HandleOnElapsedTimeChanged;
            _devicesFacade.OnRemainingTimeChanged -= HandleOnRemainingTimeChanged;
            _devicesFacade.OnSessionCompleted -= OnSessionCompleted;
            _devicesFacade.OnSessionCompleted -= HandleOnSessionCompleted;
            _devicesFacade.OnPausedFromDevice -= OnPausedFromDevice;
            _devicesFacade.OnPausedFromDevice -= HandleOnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice -= OnResumedFromDevice;
            _devicesFacade.OnResumedFromDevice -= HandleOnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice -= OnEmeregencyStoppedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice -= HandleOnEmeregencyStoppedFromDevice;
            _devicesFacade.OnReversedFromDevice -= OnReversedFromDevice;
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
            
            CurrentCycleNumber = completedCycleNumber == _startParams.CycleCount
                ? completedCycleNumber
                : (short)(completedCycleNumber + 1);
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

                    var checkPoint = PatientParamsPerCycles[cycleNumber.Value]
                        .FirstOrDefault(x => x.IterationNumber == iterationNumber.Value);
                    if (checkPoint == null) return;
                    checkPoint.HandleErrorOnCommoParamsProcessing();
                    break;
                }
                case SessionProcessingErrorCodes.PatientPressureParamsRequestError:
                case SessionProcessingErrorCodes.PatientPressureParamsRequestTimeout:
                {
                    var cycleNumber = exception.CycleNumber;
                    var iterationNumber = exception.IterationNumber;
                    if (iterationNumber == null || cycleNumber == null) return;

                    var checkPoint = PatientParamsPerCycles[cycleNumber.Value]
                        .FirstOrDefault(x => x.IterationNumber == iterationNumber.Value);
                    if (checkPoint == null) return;
                    checkPoint.HandleErrorOnPressureParamsProcessing();
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
                var cycleNumber = patientPressureParams.CycleNumber;
                var iterationNumber = patientPressureParams.IterationNumber;
                var inclinationAngle = patientPressureParams.InclinationAngle;
                
                var checkPoint = PatientParamsPerCycles[cycleNumber].FirstOrDefault(x => x.IterationNumber == iterationNumber);
                if (checkPoint == null)
                {
                    checkPoint = new CheckPointParams(
                        cycleNumber, 
                        iterationNumber,
                        inclinationAngle);
                    PatientParamsPerCycles[cycleNumber].Add(checkPoint);
                }

                checkPoint.SetPressureParams(patientPressureParams);
            }
        }

        private void HandleOnCommonPatientParamsRecieved(
            object sender, 
            CommonPatientParams commonPatientParams)
        {
            lock (_cycleDataLocker)
            {
                var cycleNumber = commonPatientParams.CycleNumber;
                var iterationNumber = commonPatientParams.IterationNumber;

                var inclinationAngle = commonPatientParams.InclinationAngle;
                
                var checkPoint = PatientParamsPerCycles[cycleNumber].FirstOrDefault(x => x.IterationNumber == iterationNumber);
                if (checkPoint == null)
                {
                    checkPoint = new CheckPointParams(
                        cycleNumber, 
                        iterationNumber, 
                        inclinationAngle);
                    PatientParamsPerCycles[cycleNumber].Add(checkPoint);
                }
                checkPoint.SetCommonParams(commonPatientParams);
            }
        }

        private void HandleOnCurrentAngleXRecieved(object sender, float value)
        {
            CurrentXAngle = value;
        }
        
        private void HandleOnRemainingTimeChanged(object sender, TimeSpan timeSpan)
        {
            RemainingTime = timeSpan;
        }

        private void HandleOnElapsedTimeChanged(object sender, TimeSpan timeSpan)
        {
            ElapsedTime = timeSpan;
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