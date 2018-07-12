using System;
using System.Collections.Generic;

namespace CardioMonitor.FileSaving.Containers.V1
{
    internal class StoredSessionV1
    { 
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор паицента
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Дата и время (UTC)
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public StoredSessionStatusV1 Status { get; set; }

        public List<StoredSessionCycleV1> Cycles { get; set; }
    }
}
