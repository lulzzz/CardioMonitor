using System.Collections.ObjectModel;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Models.Session;
using CardioMonitor.ViewModel.Base;

namespace CardioMonitor.ViewModel.Sessions
{
    public class SessionDataViewModel : Notifier, IViewModel
    {
        private PatientFullName _patientName;
        private Patient _patient;
        private SessionModel _session;
        private ICommand _saveCommand;

        public PatientFullName PatientName
        {
            get { return _patientName; }
            set
            {
                if (value != _patientName)
                {
                    _patientName = value;
                    RisePropertyChanged("PatientName");
                }
            }
        }

        public Patient Patient
        {
            get { return _patient; }
            set
            {
                if (value != _patient)
                {
                    _patient = value; 
                    PatientName = new PatientFullName
                    {
                        LastName = _patient.LastName,
                        FirstName = _patient.FirstName,
                        PatronymicName = _patient.PatronymicName,
                    };
                    RisePropertyChanged("Patient");
                    RisePropertyChanged("Patients");
                }
            }
        }

        public ObservableCollection<Patient> Patients
        {
            get { return new ObservableCollection<Patient> { Patient }; }
        }

        public SessionModel Session
        {
            get { return _session; }
            set
            {
                if (value != _session)
                {
                    _session = value;
                    RisePropertyChanged("Session");
                }
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x => SaveToFile()
                });
            }
        }


        public SessionDataViewModel()
        {
           
        }

        public async void SaveToFile()
        {
            await MessageHelper.Instance.ShowMessageAsync("Saved!");
        }

        public void Clear()
        {
            PatientName = null;
            Session = null;
            Patient = null;
        }
    }
}
