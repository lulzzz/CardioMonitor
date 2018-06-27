using System;
using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Сеанс
    /// </summary>
    public class Session
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
        public DateTime DateTimeUtc { get; set; }
        
        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status { get; set; }

        public List<SessionCycle> Cycles { get; set; }

        /// <summary>
        /// Сеанс
        /// </summary>
        public Session()
        {
            DateTimeUtc = new DateTime();
            Status = SessionStatus.NotStarted;
        }
    }

    /// <summary>
    /// Сеанс с информацией о пациента
    /// </summary>
    public class SessionWithPatientInfo
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
        /// ФИО паицента
        /// </summary>
        public string PatientFullName { get; set; }

        /// <summary>
        /// Дата и время сеанса
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status { get; set; }
        
        /// <summary>
        /// Сеанс
        /// </summary>
        public SessionWithPatientInfo()
        {
            TimestampUtc = new DateTime();
            Status = SessionStatus.NotStarted;
        }
    }
}
