using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Цикл сеанса (повторение)
    /// </summary>
    public class SessionCycle
    {
        public int Id { get; set; }
        
        public int SessionId { get; set; }

        public int CycleNumber { get; set; }

        /// <summary>
        /// Показатели пациента за сеанс
        /// </summary>
        public List<PatientParams> PatientParams { get; set; }

        //todo ecg
    }
}