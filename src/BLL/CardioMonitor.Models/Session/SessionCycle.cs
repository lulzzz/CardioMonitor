using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Цикл сеанса (повторение)
    /// </summary>
    public class SessionCycle
    {
        public SessionCycle(List<PatientParams> patientParams)
        {
            PatientParams = patientParams;
        }

        /// <summary>
        /// Показатели пациента за сенас
        /// </summary>
        public List<PatientParams> PatientParams { get; set; }
    }
}