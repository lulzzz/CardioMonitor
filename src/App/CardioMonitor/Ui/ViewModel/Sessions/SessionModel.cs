using System;
using System.Collections.ObjectModel;
using CardioMonitor.Ui.Base;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    /// <summary>
    /// Модель сеансаов для SessionView
    /// </summary>
    public class SessionModel: Notifier
    {
        private int _id;
        private int _treatmentId;
        private DateTime _dateTime;
        private SessionStatus _status;
        private ObservableCollection<PatientParams> _patientParams;

        /// <summary>
        /// Идентифкатор
        /// </summary>
        public int Id 
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    RisePropertyChanged("Id");
                }
            } 
        }

        /// <summary>
        /// Идентифкатор курса лечения
        /// </summary>
        public int TreatmentId 
        {
            get { return _treatmentId; }
            set
            {
                if (value != _treatmentId)
                {
                    _treatmentId = value;
                    RisePropertyChanged("Treatment");
                }
            }
        }

        /// <summary>
        /// Дата и время сеанса
        /// </summary>
        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                if (value != _dateTime)
                {
                    _dateTime = value;
                    RisePropertyChanged("DateTime");
                }
            }
        }

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status
        {
            get { return _status; }
            set
            {
                if (value != _status)
                {
                    _status = value;
                    RisePropertyChanged("Status");
                }
            }
        }

        /// <summary>
        /// Показатели пациента
        /// </summary>
        public ObservableCollection<PatientParams> PatientParams
        {
            get { return _patientParams; }
            set
            {
                if (value != _patientParams)
                {
                    _patientParams = value;
                    RisePropertyChanged("PatientParams");
                }
            }
        }

        /// <summary>
        /// Сеанс
        /// </summary>
        public Session Session
        {
            get
            {
                return new Session
                {
                    Id = Id,
                    TreatmentId = TreatmentId,
                    DateTime = DateTime,
                    Status = Status,
                    PatientParams = PatientParams
                };
            }
            set
            {
                Id = value.Id;
                TreatmentId = value.TreatmentId;
                DateTime = value.DateTime;
                Status = value.Status;
                PatientParams = value.PatientParams;
            }
        }

        /// <summary>
        /// Модель сеансаов для SessionView
        /// </summary>
        public SessionModel()
        {
            DateTime = new DateTime();
            Status = SessionStatus.Unknown;
            PatientParams = new ObservableCollection<PatientParams>();
        }
    }
}
