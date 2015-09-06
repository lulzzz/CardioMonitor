using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Infrastructure.Models.Patients;
using CardioMonitor.Infrastructure.Models.Session;
using CardioMonitor.Infrastructure.Repository.Files;
using CardioMonitor.Infrastructure.Resources;
using CardioMonitor.Infrastructure.Ui.Base;

namespace CardioMonitor.Infrastructure.Ui.Sessions
{
    public class SessionDataViewModel : Notifier, IViewModel
    {
        private PatientFullName _patientName;
        private Patient _patient;
        private SessionModel _session;
        private ICommand _saveCommand;
        private bool _isReadOnly;

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
                    if (_patient != null)
                    {

                        PatientName = new PatientFullName
                        {
                            LastName = _patient.LastName,
                            FirstName = _patient.FirstName,
                            PatronymicName = _patient.PatronymicName,
                        };   
                    }
                    else
                    {
                        PatientName = new PatientFullName();
                    }
                    RisePropertyChanged("Patient");
                    RisePropertyChanged("Patients");
                }
            }
        }

        public ObservableCollection<Patient> Patients
        {
            get { return Patient != null 
                          ? new ObservableCollection<Patient> { Patient }
                          : new ObservableCollection<Patient>(); }
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

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RisePropertyChanged("IsReadOnly");
            }
        }

        public SessionDataViewModel()
        {
            IsReadOnly = true;
        }

        public async void SaveToFile()
        {
            var saveDialog = new SaveFileDialog { Filter = Localisation.FileRepository_SeansFileFilter };
            var result = saveDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var message = String.Empty;
                try
                {
                    FileRepository.SaveToFile(saveDialog.FileName, Patient, Session.Session);
                }
                catch (ArgumentNullException)
                {
                    message = Localisation.ArgumentNullExceptionMessage;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    await MessageHelper.Instance.ShowMessageAsync(message);
                }
            }
        }

        public async void OpenFromFile()
        {
            var loadDialog = new OpenFileDialog { CheckFileExists = true, Filter = Localisation.FileRepository_SeansFileFilter };
            var result = loadDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var message = String.Empty;
                try
                {
                    var container = FileRepository.LoadFromFile(loadDialog.FileName);
                    Session = new SessionModel { Session = container.Session };
                    Patient = container.Patient;
                }
                catch (ArgumentNullException)
                {
                    message = Localisation.ArgumentNullExceptionMessage;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    await MessageHelper.Instance.ShowMessageAsync(message);
                }
            }
        }

        public void Clear()
        {
            PatientName = null;
            Session = null;
            Patient = null;
        }
    }
}
