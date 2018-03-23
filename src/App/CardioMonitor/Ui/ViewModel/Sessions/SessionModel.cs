using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Ui.Base;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    /// <summary>
    /// Модель сеансаов для Session (обертка)
    /// </summary>
    public class SessionModel: Notifier
    {
        private int _id;
        private int _treatmentId;
        private DateTime _dateTime;
        private SessionStatus _status;
        private ObservableCollection<SessionCycleViewModel> _cycles;

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
        public ObservableCollection<SessionCycleViewModel> Cycles
        {
            get { return _cycles; }
            set
            {
                if (value != _cycles)
                {
                    _cycles = value;
                    RisePropertyChanged("Cycles");
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
                    Cycles = new List<SessionCycle>(Cycles.Select(x => new SessionCycle
                    {
                        CycleNumber = x.CycleNumber,
                        PatientParams = new List<PatientParams>(x.PatientParams)
                    }))
                };
            }
            set
            {
                Id = value.Id;
                TreatmentId = value.TreatmentId;
                DateTime = value.DateTime;
                Status = value.Status;
                Cycles = new ObservableCollection<SessionCycleViewModel>(value.Cycles.Select(x => new SessionCycleViewModel
                {
                    CycleNumber = x.CycleNumber,
                    PatientParams = new ObservableCollection<PatientParams>(x.PatientParams)
                }));
            }
        }

        /// <summary>
        /// Модель сеансаов для Session
        /// </summary>
        public SessionModel()
        {
            DateTime = new DateTime();
            Status = SessionStatus.NotStarted;
            Cycles = new ObservableCollection<SessionCycleViewModel>();
        }
    }
}
