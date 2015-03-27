using System;
using System.Collections.ObjectModel;

namespace CardioMonitor.Core.Models.Session
{
    /// <summary>
    /// Сеанс
    /// </summary>
    [Serializable]
    public class Session
    {
        /// <summary>
        /// Идентифкатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентифкатор курса лечения
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// Дата и время
        /// </summary>
        public DateTime DateTime { get; set; }
        
        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status { get; set; }

        /// <summary>
        /// Показтели пациента за сенас
        /// </summary>
        public ObservableCollection<PatientParams> PatientParams { get; set; }

        /// <summary>
        /// Сеанс
        /// </summary>
        public Session()
        {
            DateTime = new DateTime();
            Status = SessionStatus.Unknown;
            PatientParams = new ObservableCollection<PatientParams>();
        }
    }
}
