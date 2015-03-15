using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CardioMonitor.Core;

namespace CardioMonitor.Patients.Session
{
    public class SessionModel: Notifier
    {
        private int _id;
        private int _treatmentId;
        private DateTime _dateTime;
        private SessionStatus _status;
        private ObservableCollection<PatientParams> _patientParams;

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

        public SessionModel()
        {
            DateTime = new DateTime();
            Status = SessionStatus.Unknown;
            PatientParams = new ObservableCollection<PatientParams>();
        }


    }
}
