﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.Communication;
using CardioMonitor.Ui.ViewModel.Sessions;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Patients
{
    public class PatientsViewModel : Notifier, IStoryboardPageViewModel
    {
        private readonly IPatientsService _patientsService;
        private int _seletedPatientIndex;
        private Patient _selectePatient;
        private ObservableCollection<Patient> _patients; 

        private ICommand _addNewPatientCommand;
        private ICommand _deletePatientCommand;
        private ICommand _editPateintCommand;
        private ICommand _patientSearchCommand;
        private ICommand _opentSesionsCommand;

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
                       (_editPateintCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => null != SelectedPatient,
                               ExecuteDelegate = x => EditPatient()
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
                               ExecuteDelegate = x=> DeletePatient()
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
        

        public PatientsViewModel(IPatientsService patientsService)
        {
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));

            Patients = new ObservableCollection<Patient>();
        }

        private void AddNewPatient()
        {
            PageTransitionRequested?.Invoke(this, 
                new TransitionRequest(
                    PageIds.PatientPageId,
                    new PatientPageContext { Patient = new Patient(), Mode = AccessMode.Create }));
        }

        private void EditPatient()
        {
            PageTransitionRequested?.Invoke(this,
                new TransitionRequest(
                    PageIds.PatientPageId,
                    new PatientPageContext {Patient = SelectedPatient, Mode = AccessMode.Edit}));
        }

        private async void DeletePatient()
        {
            var result = await MessageHelper.Instance.ShowMessageAsync(Localisation.PatientsViewModel_DeletePatientQuestion,
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative == result)
            {
                var exceptionMassage = String.Empty;
                if (null != SelectedPatient)
                {
                    try
                    {
                        _patientsService.Delete(SelectedPatient.Id);
                        Patients.Remove(SelectedPatient);
                        SelectedPatient = null;
                    }
                    catch (Exception ex)
                    {
                        exceptionMassage = ex.Message;
                    }
                }
                if (!String.IsNullOrEmpty(exceptionMassage))
                {
                    await MessageHelper.Instance.ShowMessageAsync(exceptionMassage);
                }
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
            PageTransitionRequested?.Invoke(this,
                new TransitionRequest(PageIds.SessionsPageId, new SessionsPageContext
                {
                    Patient = SelectedPatient
                }));
        }


        public void Dispose()
        {
        }

        private async Task UpdatePatientsSafeAsync()
        {
            var message = String.Empty;
            var a = await MessageHelper.Instance.ShowProgressDialogAsync("Загрузка списка пациентов...")
                .ConfigureAwait(true);
            try
            {
                var patients = await Task.Factory.StartNew(() => _patientsService.GetAll()).ConfigureAwait(true);
                Patients = patients != null
                    ? new ObservableCollection<Patient>(patients)
                    : new ObservableCollection<Patient>();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                await a.CloseAsync().ConfigureAwait(true);
            }
            if (!String.IsNullOrEmpty(message))
            {

                await MessageHelper.Instance.ShowMessageAsync(message);
            }
        }

        #region IStoryboardViewModel




        public Guid PageId { get; set; }

        public Guid StoryboardId { get; set; }

        public async Task OpenAsync(IStoryboardPageContext context)
        {
            await UpdatePatientsSafeAsync().ConfigureAwait(false);
        }

        public Task<bool> CanLeaveAsync()
        {
            return Task.FromResult(true);
        }

        public Task LeaveAsync()
        {
            return Task.CompletedTask;
        }

        public Task ReturnAsync(IStoryboardPageContext context)
        {
            return Task.CompletedTask;
        }

        public Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync()
        {
            return Task.CompletedTask;
        }

        public event EventHandler PageCanceled;

        public event EventHandler PageCompleted;

        public event EventHandler PageBackRequested;

        public event EventHandler<TransitionRequest> PageTransitionRequested;

        public event EventHandler CanCloseChanged;

        public event EventHandler CanLeaveChanged;

        #endregion
    }
}
