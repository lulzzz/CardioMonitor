using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Infrastructure.WpfCommon.Base;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    /// <summary>
    /// Модель сеансаов для Session (обертка)
    /// </summary>
    public class SessionModel: Notifier
    {
        private int _id;
        private int _patientId;
        private DateTime _dateTime;
        private SessionStatus _status;
        private ObservableCollection<SessionCycleViewModel> _cycles;

        /// <summary>
        /// Идентифкатор
        /// </summary>
        public int Id 
        {
            get => _id;
            set
            {
                if (value == _id) return;
                _id = value;
                RisePropertyChanged(nameof(Id));
            } 
        }

        /// <summary>
        /// Идентифкатор курса лечения
        /// </summary>
        public int PatientId 
        {
            get => _patientId;
            set
            {
                if (value == _patientId) return;
                _patientId = value;
                RisePropertyChanged(nameof(PatientId));
            }
        }

        /// <summary>
        /// Дата и время сеанса
        /// </summary>
        public DateTime DateTime
        {
            get => _dateTime;
            set
            {
                if (value == _dateTime) return;
                _dateTime = value;
                RisePropertyChanged(nameof(DateTime));
            }
        }

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                RisePropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Показатели пациента
        /// </summary>
        public ObservableCollection<SessionCycleViewModel> Cycles
        {
            get => _cycles;
            set
            {
                if (value == _cycles) return;
                _cycles = value;
                RisePropertyChanged(nameof(Cycles));
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
                    PatientId = PatientId,
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
                PatientId = value.PatientId;
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
