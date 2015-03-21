using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Core.Repository.DataBase;
using CardioMonitor.ViewModel.Communication;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.ViewModel.Patients
{
    public class PatientsViewModel : Notifier
    {
        private int _seletedPatientIndex;
        private Core.Models.Patients.Patient _selectePatient;
        private ObservableCollection<Core.Models.Patients.Patient> _patients; 

        private ICommand _addNewPatientCommand;
        private ICommand _deletePatientCommand;
        private ICommand _editPateintCommand;
        private ICommand _openPatientCommand;
        private ICommand _patientSearchCommand;
        private ICommand _opentSesionsCommand;
        private ICommand _showTreatmentResultsCommand;
        private ICommand _openSessionCommand;

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

        public Core.Models.Patients.Patient SelectedPatient
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

        public ObservableCollection<Core.Models.Patients.Patient> Patients
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
                       (_editPateintCommand =
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

        public ICommand OpenSessionsCommand
        {
            get
            {
                return _opentSesionsCommand ??
                       (_opentSesionsCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => null != SelectedPatient,
                               ExecuteDelegate = x => OpenSessions()
                           });
            }
        }

        public ICommand ShowResultsCommand
        {
            get
            {
                return _showTreatmentResultsCommand ??
                       (_showTreatmentResultsCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => null != SelectedPatient,
                               ExecuteDelegate = x => ShowResults()
                           });
            }
        }

        public ICommand OpenSessionCommand
        {
            get
            {
                return _openSessionCommand ?? (_openSessionCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x => OpenSession()
                });
            }
        }

        //temporary not used
        public EventHandler OpenPatienEvent { get; set; }
        public EventHandler AddEditPatient { get; set; }
        public EventHandler OpenSessionsHandler { get; set; }
        public EventHandler ShowTreatmentResults { get; set; }
        public EventHandler OpenSessionHandler { get; set; }

        public PatientsViewModel()
        {
            Patients = new ObservableCollection<Core.Models.Patients.Patient>();
        }

        private void AddNewPatient()
        {
            var handler = AddEditPatient;
            if (null != handler)
            {
                handler(this, new PatientEventArgs {Patient = new Core.Models.Patients.Patient(), Mode = AccessMode.Create});
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
                var isDeleted = false;
                if (null != SelectedPatient)
                {
                    try
                    {
                        DataBaseRepository.Instance.DeletePatient(SelectedPatient.Id);
                        isDeleted = true;
                        Patients.Remove(SelectedPatient);
                        SelectedPatient = null;
                    }
                    catch
                    {
                        isDeleted = false;
                    }
                }
                if (!isDeleted)
                {
                    await MessageHelper.Instance.ShowMessageAsync("Не удалось удалить пациента");
                }
            }
        }

        private void OpenPatientsTreatment(object sender)
        {
            var patient = sender as Core.Models.Patients.Patient;
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

        private void OpenSessions()
        {
            var handler = OpenSessionsHandler;
            if (null != handler)
            {
                handler(this, null);
            }
        }

        private void ShowResults()
        {
            var handler = ShowTreatmentResults;
            if (null != handler)
            {
                handler(this, null);
            }
        }

        private void OpenSession()
        {
            var handler = OpenSessionHandler;
            if (null != handler)
            {
                handler(this, null);
            }
        }
    }
}
