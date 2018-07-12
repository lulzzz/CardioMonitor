using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing
{
    /// <summary>
    /// Значение параметров пациента в контрольной точке
    /// </summary>
    public class CheckPointParams : INotifyPropertyChanged
    {

        public CheckPointParams(
            short cycleNumber, 
            short iterationNumber, 
            float inclinationAngle)
        {
            CycleNumber = cycleNumber;
            IterationNumber = iterationNumber;
            InclinationAngle = inclinationAngle;
            
            HeartRate = new DeviceValue<short>();
            RespirationRate = new DeviceValue<short>();
            Spo2 = new DeviceValue<short>();
            SystolicArterialPressure = new DeviceValue<short>();
            DiastolicArterialPressure = new DeviceValue<short>();
            AverageArterialPressure = new DeviceValue<short>();
        }

        internal void SetCommonParams([NotNull] CommonPatientParams patientParams)
        {
            if (patientParams == null) throw new ArgumentNullException(nameof(patientParams));
            IsAnyValueObtained = true;
            HeartRate = new DeviceValue<short>(patientParams.HeartRate);
            RespirationRate = new DeviceValue<short>(patientParams.RespirationRate);
            Spo2 = new DeviceValue<short>(patientParams.Spo2);
        }

        internal void SetPressureParams([NotNull] PatientPressureParams pressureParams)
        {
            if (pressureParams == null) throw new ArgumentNullException(nameof(pressureParams));
            IsAnyValueObtained = true;
            SystolicArterialPressure = new DeviceValue<short>(pressureParams.SystolicArterialPressure);
            DiastolicArterialPressure = new DeviceValue<short>(pressureParams.DiastolicArterialPressure);
            AverageArterialPressure = new DeviceValue<short>(pressureParams.AverageArterialPressure);
        }

        internal void HandleErrorOnCommoParamsProcessing()
        {
            // на тот случай, если данные в рамках итерации были получены, но итерация не завершилась, а ошибка возникла
            if (HeartRate.Status == DeviceValueStatus.Obtained) return;
            
            HeartRate = new DeviceValue<short>(DeviceValueStatus.ErrorOccured);
            RespirationRate = new DeviceValue<short>(DeviceValueStatus.ErrorOccured);
            Spo2 = new DeviceValue<short>(DeviceValueStatus.ErrorOccured);
        }
        
        internal void HandleErrorOnPressureParamsProcessing()
        {
            // на тот случай, если данные в рамках итерации были получены, но итерация не завершилась, а ошиюба возникла
            if (AverageArterialPressure.Status == DeviceValueStatus.Obtained) return;
            
            SystolicArterialPressure = new DeviceValue<short>(DeviceValueStatus.ErrorOccured);
            DiastolicArterialPressure = new DeviceValue<short>(DeviceValueStatus.ErrorOccured);
            AverageArterialPressure = new DeviceValue<short>(DeviceValueStatus.ErrorOccured);
        }

        #region Fields

        /// <summary>
        /// Номер цикла
        /// </summary>
        public short CycleNumber { get; }
        
        /// <summary>
        /// Номер итерации
        /// </summary>
        public short IterationNumber { get; }
        
        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public float InclinationAngle { get; }
        
        public bool IsAnyValueObtained { get; private set; }

        /// <summary>п
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public DeviceValue<short> HeartRate
        {
            get => _heartRate;
            set
            {
                _heartRate = value;
                OnPropertyChanged(nameof(HeartRate));
            }
        }
        private DeviceValue<short> _heartRate;

        /// <summary>
        /// Частота дыхания (ЧД)
        /// </summary>
        public DeviceValue<short> RespirationRate
        {
            get => _respirationRate;
            set
            {
                _respirationRate = value;
                OnPropertyChanged(nameof(RespirationRate));
            }
        }
        private DeviceValue<short> _respirationRate;

        /// <summary>
        /// SPO2
        /// </summary>
        public DeviceValue<short> Spo2
        {
            get => _spo2;
            set
            {
                _spo2 = value;
                OnPropertyChanged(nameof(Spo2));
            }
        }
        private DeviceValue<short> _spo2;


        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public DeviceValue<short> SystolicArterialPressure
        {
            get => _systolicArterialPressure;
            set { _systolicArterialPressure = value; 
                OnPropertyChanged(nameof(SystolicArterialPressure));}
        }
        private DeviceValue<short> _systolicArterialPressure;

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public DeviceValue<short> DiastolicArterialPressure
        {
            get => _diastolicArterialPressure;
            set { _diastolicArterialPressure = value; 
                OnPropertyChanged(nameof(DiastolicArterialPressure));}
        }
        private DeviceValue<short> _diastolicArterialPressure;

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public DeviceValue<short> AverageArterialPressure
        {
            get => _averageArterialPressure;
            set { _averageArterialPressure = value; 
                OnPropertyChanged(nameof(AverageArterialPressure));}
        }
        private DeviceValue<short> _averageArterialPressure;
        
        #endregion
        
        #region Rise events 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}