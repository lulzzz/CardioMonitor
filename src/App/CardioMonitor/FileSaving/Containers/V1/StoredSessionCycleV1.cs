using System.Collections.Generic;

namespace CardioMonitor.FileSaving.Containers.V1
{
    internal class StoredSessionCycleV1
    {
        public int Id { get; set; }

        public int SessionId { get; set; }

        public int CycleNumber { get; set; }

        /// <summary>
        /// Показатели пациента за сеанс
        /// </summary>
        public List<StoredPatientParamsV1> PatientParams { get; set; }
    }
}
