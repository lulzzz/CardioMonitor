using System.Collections.Generic;
using CardioMonitor.Core.Models.Session;

namespace CardioMonitor.Core.Repository.Monitor
{
    /// <summary>
    /// Репозиторй для получения данных с монитора
    /// </summary>
    public class MonitorRepository
    {
        #region Singletone

        private static MonitorRepository _instance;
        private static readonly object SyncObject = new object();

        /// <summary>
        /// Репозиторй для получения данных с монитора
        /// </summary>
        public static MonitorRepository Instance
        {
            get
            {
                if (null != _instance)
                {
                    return _instance;
                }
                lock (SyncObject)
                {
                    return _instance ?? (_instance = new MonitorRepository());
                }
            }
        }

        #endregion

        private readonly List<PatientParams> _patientParams;

        private int _index;

        /// <summary>
        /// Индекс для эмуляции доступа к данным
        /// </summary>
        /// <remarks>Чтобы можно было бесконечно считывать, не выходя за границы</remarks>
        private int Index
        {
            get
            {
                if (_index + 1 > _patientParams.Count)
                {
                    _index = 0;
                }
                return _index++;
            }

        }

        /// <summary>
        /// Репозиторй для получения данных с монитора
        /// </summary>
        private MonitorRepository()
        {
#if Debug_Monitor || RELEASE
            MonitorDataReader.StartConnection();
#endif
            //заполняем псведоданнымиы
            _patientParams = new List<PatientParams>
            {
                new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 0,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                },
                new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 10.5,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                },
                new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 21,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                },
                new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 30,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                },
                new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 21,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                },
                new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 10.5,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                },
                new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 0,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                }
            };
        }
        
        /// <summary>
        /// Возвращает показатели пациента
        /// </summary>
        /// <returns>Показатели пациента</returns>
        /// <remarks>Эмулирует работу с монитором. Сюда следует поместить логику считывания данных с монитора</remarks>
        public PatientParams GetPatientParams()
        {

#if Debug_Monitor || RELEASE
            var patientParametrs = MonitorDataReader.GetPatientParams(MonitorDataReader.Listener);
            return patientParametrs;
#else
            return _patientParams[Index];
#endif
        }
    }
}
