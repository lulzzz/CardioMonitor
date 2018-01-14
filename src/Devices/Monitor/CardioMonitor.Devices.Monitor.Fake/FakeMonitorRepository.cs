using System.Collections.Generic;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor.Fake
{
    public class FakeMonitorRepository : IMonitorController
    {

        private readonly List<PatientParams> _patientParams;
        
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
        private int _index;

        public FakeMonitorRepository()
        { //заполняем псведоданнымиы
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

        public Task<bool> PumpCuffAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<PatientParams> GetPatientParamsAsync()
        {
            return Task.Factory.StartNew(() => _patientParams[Index]);
        }

        public Task<PatientPressureParams> GetPatientPressureParams()
        {
            throw new System.NotImplementedException();
        }
    }
}