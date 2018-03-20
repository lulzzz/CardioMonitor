using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;
using PatientPressureParams = CardioMonitor.BLL.SessionProcessing.DeviceFacade.PatientPressureParams;

namespace CardioMonitor.BLL.SessionProcessing
{
    public class SessionProcessor :  INotifyPropertyChanged, IDisposable
    {
        private const double Tolerance = 1e-9;
        
        [NotNull] private readonly SessionParams _startParams;

        [NotNull]
        private readonly IDevicesFacade _devicesFacade;
        
        [NotNull]
        private readonly object _cycleDataLocker = new object();

        public SessionProcessor(
            [NotNull] SessionParams startParams,
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController,
            [NotNull] IWorkerController workerController)
        {
            if (startParams == null) throw new ArgumentNullException(nameof(startParams));
            if (bedController == null) throw new ArgumentNullException(nameof(bedController));
            if (monitorController == null) throw new ArgumentNullException(nameof(monitorController));
            if (workerController == null) throw new ArgumentNullException(nameof(workerController));
            _startParams = startParams;

            _devicesFacade = new DevicesFacade(
                startParams,
                bedController,
                monitorController,
                workerController);
            
            CycleData = new ObservableCollection<CheckPointParams>[startParams.CycleCount];
            _devicesFacade.OnException += OnException;
            _devicesFacade.OnCycleCompleted += OnCycleCompletedHandler;
            _devicesFacade.OnSessionCompleted += OnSessionCompleted;
            _devicesFacade.OnPausedFromDevice += OnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice += OnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice += OnEmeregencyStoppedFromDevice;
            _devicesFacade.OnReversedFromDevice += OnReversedFromDevice;
            _devicesFacade.OnCurrentAngleXRecieved += DevicesFacadeOnOnCurrentAngleXRecieved;
            _devicesFacade.OnCommonPatientParamsRecieved += DevicesFacadeOnOnCommonPatientParamsRecieved;
            _devicesFacade.OnPatientPressureParamsRecieved += DevicesFacadeOnOnPatientPressureParamsRecieved;
            CurrentCycleNumber = 0;
        }
    
        private IReadOnlyList<ObservableCollection<CheckPointParams>> CycleData { get; }

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

        private float _currentXAngle;

        public float CurrentXAngle
        {
            get => _currentXAngle;
            set
            {
                var oldValue = _currentCycleNumber;
                _currentXAngle = value;
                if (oldValue - value > Tolerance)
                {
                    RisePropertyChanged(nameof(CurrentXAngle));
                }
            }
        }

        #region Events

        public event EventHandler<SessionProcessingException> OnException;
       
        
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

        
        public Task StartAsync()
        {
            return _devicesFacade.StartAsync();
        }

        public Task EmeregencyStopAsync()
        {
            return _devicesFacade.EmergencyStopAsync();
        }

        public Task PauseAsync()
        {
            return _devicesFacade.PauseAsync();
        }

        public Task ResumeAsync()
        {
            return _devicesFacade.StartAsync();
        }

        public Task ReverseAsync()
        {
            return _devicesFacade.ProcessReverseRequestAsync();
        }

        public void EnableAutoPumping()
        {
            _devicesFacade.EnableAutoPumping();
        }

        public void DisableAutoPumping()
        {
            _devicesFacade.DisableAutoPumping();
        }

        public void Dispose()
        {
            _devicesFacade.OnException -= OnException;
            _devicesFacade.OnCycleCompleted -= OnCycleCompletedHandler;
            _devicesFacade.OnSessionCompleted -= OnSessionCompleted;
            _devicesFacade.OnPausedFromDevice -= OnPausedFromDevice;
            _devicesFacade.OnResumedFromDevice -= OnResumedFromDevice;
            _devicesFacade.OnEmeregencyStoppedFromDevice -= OnEmeregencyStoppedFromDevice;
            _devicesFacade.OnReversedFromDevice -= OnReversedFromDevice;
            _devicesFacade.OnCurrentAngleXRecieved -= DevicesFacadeOnOnCurrentAngleXRecieved;
            _devicesFacade.OnCommonPatientParamsRecieved -= DevicesFacadeOnOnCommonPatientParamsRecieved;
            _devicesFacade.OnPatientPressureParamsRecieved -= DevicesFacadeOnOnPatientPressureParamsRecieved;
        }

        #endregion

        private void OnCycleCompletedHandler(object sender, short completedCycleNumber)
        {
            CurrentCycleNumber = completedCycleNumber == _startParams.CycleCount
                ? completedCycleNumber
                : (short)(completedCycleNumber + 1);
        }
        
        private void DevicesFacadeOnOnPatientPressureParamsRecieved(
            object sender,
            PatientPressureParams patientPressureParams)
        {
            lock (_cycleDataLocker)
            {
                var cycleNumber = patientPressureParams.CycleNumber;
                var iterationNumber = patientPressureParams.IterationNumber;

                var checkPoint = CycleData[cycleNumber].FirstOrDefault(x => x.IterationNumber == iterationNumber);
                if (checkPoint == null)
                {
                    checkPoint = new CheckPointParams(cycleNumber, iterationNumber);
                    CycleData[cycleNumber].Add(checkPoint);
                }

                checkPoint.SetPressureParams();
            }
        }

        private void DevicesFacadeOnOnCommonPatientParamsRecieved(
            object sender, 
            CommonPatientParams commonPatientParams)
        {
            lock (_cycleDataLocker)
            {
                var cycleNumber = commonPatientParams.CycleNumber;
                var iterationNumber = commonPatientParams.IterationNumber;

                var checkPoint = CycleData[cycleNumber].FirstOrDefault(x => x.IterationNumber == iterationNumber);
                if (checkPoint == null)
                {
                    checkPoint = new CheckPointParams(cycleNumber, iterationNumber);
                    CycleData[cycleNumber].Add(checkPoint);
                }
                checkPoint.SetCommonParams();
            }
        }

        private void DevicesFacadeOnOnCurrentAngleXRecieved(object sender, float value)
        {
            CurrentXAngle = value;
        }
    }

    public class CheckPointParams
    {
        public short CycleNumber { get; }
        
        public short IterationNumber { get; }
        
        public bool IsAnyValueObtained { get; private set; }

        public CheckPointParams(short cycleNumber, short iterationNumber)
        {
            CycleNumber = cycleNumber;
            IterationNumber = iterationNumber;
        }

        public void SetCommonParams()
        {
            IsAnyValueObtained = true;
        }

        public void SetPressureParams()
        {
            IsAnyValueObtained = true;
        }
    }

    public class DeviceValue<T>
    {
        public bool IsValueObtained { get; }
        
        public bool IsErrorOccured { get; }
        
        public T Value { get; set; }

        public DeviceValue()
        {
            IsValueObtained = false;
            IsErrorOccured = false;
            Value = default(T);
        }
        
        public DeviceValue(T value)
        {
            Value = value;
            IsValueObtained = true;
            IsErrorOccured = false;
        }

        public DeviceValue(bool isValueObtained, bool isErrorOccured)
        {
            IsValueObtained = isValueObtained;
            IsErrorOccured = isErrorOccured;
        }
    }
}