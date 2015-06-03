using System.Collections.Generic;
using CardioMonitor.Core.Models.Session;
using CardioMonitor.MonitorConnection;

namespace CardioMonitor.Core.Repository.Monitor
{
    /// <summary>
    /// Репозиторй для получения данных с монитора
    /// </summary>
    public class MonitorRepository
    {
        private static MonitorRepository _instance;
        private static readonly object SyncObject = new object();

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
            MonitorConnection.MonitorConnection.StartConnection();
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

        /// <summary>
        /// Возвращает показатели пациента
        /// </summary>
        /// <returns>Показатели пациента</returns>
        /// <remarks>Эмулирует работу с монитором. Сюда следует поместить логику считывания данных с монитора</remarks>
        public PatientParams GetPatientParams()
        {

            //var _patientParametrs =  MonitorConnection.StartTCPConnection(MonitorConnection.Listener);
            var _patientParametrs = MonitorConnection.MonitorConnection.StartTCPConnection(MonitorConnection.MonitorConnection.Listener);
            return _patientParametrs;
            //return _patientParams[Index];
        }
    }
}
