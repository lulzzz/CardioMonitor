using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Core;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.Patients
{
    public class PatientsViewModel : Notifier
    {
        private int _seletedPatientIndex;
        private Patient _selectePatient;
        private ObservableCollection<Patient> _patients; 

        private ICommand _addNewPatientCommand;
        private ICommand _deletePatientCommand;
        private ICommand _editPateintCommand;
        private ICommand _openPatientCommand;
        private ICommand _patientSearchCommand;

        public int SelectedPatientIndex
        {
            get { return _seletedPatientIndex; }
            set
            {
                if (value != _seletedPatientIndex)
                {
                    _seletedPatientIndex = value;
                    RisePropertyChanged("SelectedPatientIndex");
                }
            }
        }

        public Patient SelectedPatient
        {
            get { return _selectePatient; }
            set
            {
                if (value != _selectePatient)
                {
                    _selectePatient = value;
                    RisePropertyChanged("SelectedPatient");
                }
            }
        }

        public ObservableCollection<Patient> Patients
        {
            get { return _patients; }
            set
            {
                if (value != _patients)
                {
                    _patients = value;
                    RisePropertyChanged("Patients");
                }
            }
        }

        public ICommand AddNewPatientCommand
        {
            get
            {
                return _addNewPatientCommand ??
                       (_addNewPatientCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => true,
                               ExecuteDelegate = x => AddNewPatient()
                           });
            }
        }

        public ICommand EditPatientCommand
        {
            get
            {
                return _editPateintCommand ??
                       (_addNewPatientCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => null != SelectedPatient,
                               ExecuteDelegate = x => EditPatient(x)
                           });
            }
        }

        public ICommand DeletePatientCommnad
        {
            get
            {
                return _deletePatientCommand ??
                       (_deletePatientCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => null != SelectedPatient,
                               ExecuteDelegate = x=> DeletePatient(x)
                           });
            }
        }

        public ICommand OpenPatientCommand
        {
            get
            {
                return _openPatientCommand ??
                       (_openPatientCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => null != SelectedPatient,
                               ExecuteDelegate = x => OpenPatientsTreatment(x)
                           });
            }
        }

        public ICommand PatientSearchCommand
        {
            get
            {
                return _patientSearchCommand ??
                       (_patientSearchCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => CanSearch(x),
                               ExecuteDelegate = x => PatientSearch(x)
                           });
            }
        }

        public EventHandler OpenPatienEvent { get; set; }
        public EventHandler AddEditPatient { get; set; }

        public PatientsViewModel()
        {
            Patients = new ObservableCollection<Patient>();
            Patients.Add(new Patient { FirstName = "Maxim", Id = 1, LastName = "Markelow", PatronymicName = "Алекснадрович" });
            Patients.Add(new Patient { FirstName = "Igor", Id = 1, LastName = "Markelow", PatronymicName = "Алекснадрович" });
            Patients.Add(new Patient { FirstName = "Artem", Id = 1, LastName = "Popov", PatronymicName = "Dmitrievich" });
        }

        private void AddNewPatient()
        {
            var handler = AddEditPatient;
            if (null != handler)
            {
                handler(this, new PatientEventArgs {Patient = new Patient(), Mode = AccessMode.Create});
            }
        }

        private async void EditPatient(object sender)
        {
            var handler = AddEditPatient;
            if (null != handler)
            {
                handler(this, new PatientEventArgs { Patient = SelectedPatient, Mode = AccessMode.Edit });
            }
        }

        private async void DeletePatient(object sender)
        {
            var result = await MessageHelper.Instance.ShowMessageAsync("Вы уверены, что хотите удалить пациента?",
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative == result)
            {
                var patient = sender as Patient;
                if (null != patient)
                {
                    Patients.Remove(patient);
                }
                else
                {
                    await MessageHelper.Instance.ShowMessageAsync("Не удалось удалить пациента");
                }
            }
        }

        private void OpenPatientsTreatment(object sender)
        {
            var patient = sender as Patient;
            if (null == patient) { return; }
            //here we send id of patient and
            var handler = OpenPatienEvent;
            if (null != handler)
            {
                handler(this, new CardioEventArgs(patient.Id));
            }
        }
        
        private void PatientSearch(object sender)
        {
            MessageHelper.Instance.ShowMessageAsync(sender.ToString());
        }

        public void CancelSearch()
        {
            MessageHelper.Instance.ShowMessageAsync("Cancel");
        }

        private bool CanSearch(object sender)
        {
            var searchQuery = sender as string;
            if (null == searchQuery)
            {
                return false;
            }
            return !String.IsNullOrWhiteSpace(searchQuery);
        }
    }
}
