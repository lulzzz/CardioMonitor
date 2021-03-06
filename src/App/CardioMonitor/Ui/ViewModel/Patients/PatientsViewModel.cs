﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.EventHandlers.Patients;
using CardioMonitor.Infrastructure.WpfCommon.Base;
using CardioMonitor.Infrastructure.WpfCommon.Communication;
using CardioMonitor.Resources;
using CardioMonitor.Ui.ViewModel.Sessions;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;

namespace CardioMonitor.Ui.ViewModel.Patients
{
    public class PatientsViewModel : Notifier, IStoryboardPageViewModel
    {
        #region  Events
        
        private readonly IEventBus _eventBus;
        private readonly PatientAddedEventHandler _patientAddedEventHandler;
        private readonly PatientChangedEventHandler _patientChangedEventHandler;

        private bool _isPatientListChanged;

        #endregion

        private readonly IPatientsService _patientsService;
        [NotNull]
        private readonly ToastNotifications.Notifier _notifier;
        [NotNull] private readonly ILogger _logger;
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
            get => _seletedPatientIndex;
            set
            {
                if (value == _seletedPatientIndex) return;
                _seletedPatientIndex = value;
                RisePropertyChanged(nameof(SelectedPatientIndex));
            }
        }

        public Patient SelectedPatient
        {
            get => _selectePatient;
            set
            {
                if (value != _selectePatient)
                {
                    _selectePatient = value;
                    RisePropertyChanged(nameof(SelectedPatient));
                }
            }
        }

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set
            {
                if (value == _patients) return;
                _patients = value;
                RisePropertyChanged(nameof(Patients));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RisePropertyChanged(nameof(IsBusy));
            }
        }
        private bool _isBusy;

        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value; 
                RisePropertyChanged(nameof(BusyMessage));
            }
        }
        private string _busyMessage;

        public ICommand AddNewPatientCommand
        {
            get
            {
                return _addNewPatientCommand ??
                       (_addNewPatientCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => true,
                               ExecuteDelegate = async x => await AddPatientAsync()
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
                               ExecuteDelegate = async x => await EditPatientAsync()
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
                               ExecuteDelegate = async x=> await DeletePatientAsync()
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
                               ExecuteDelegate = async x => await OpenSessionsAsync()
                           });
            }
        }
        

        public PatientsViewModel(
            IPatientsService patientsService,
            [NotNull] ILogger logger,
            [NotNull] IEventBus eventBus,
            [NotNull] PatientAddedEventHandler patientAddedEventHandler,
            [NotNull] PatientChangedEventHandler patientChangedEventHandler, 
            [NotNull] ToastNotifications.Notifier notifier)
        {
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _patientAddedEventHandler = patientAddedEventHandler ?? throw new ArgumentNullException(nameof(patientAddedEventHandler));
            _patientChangedEventHandler = patientChangedEventHandler ?? throw new ArgumentNullException(nameof(patientChangedEventHandler));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _patientAddedEventHandler.PatientAdded += (sender, i) => _isPatientListChanged = true;
            _patientChangedEventHandler.PatientChanged += (sender, i) => _isPatientListChanged = true;

            _isPatientListChanged = false;

            Patients = new ObservableCollection<Patient>();
        }

        private async Task AddPatientAsync()
        {
            await PageTransitionRequested.InvokeAsync(this, 
                new TransitionRequest(
                    PageIds.PatientPageId,
                    new PatientPageContext { Patient = new Patient(), Mode = AccessMode.Create }))
                .ConfigureAwait(false);
        }

        private async Task EditPatientAsync()
        {
            await PageTransitionRequested.InvokeAsync(this,
                new TransitionRequest(
                    PageIds.PatientPageId,
                    new PatientPageContext {Patient = SelectedPatient, Mode = AccessMode.Edit}))
                .ConfigureAwait(false);
        }

        private async Task DeletePatientAsync()
        {
            var result = await MessageHelper.Instance.ShowMessageAsync(
                Localisation.PatientsViewModel_DeletePatientQuestion,
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative != result) return;

            if (null == SelectedPatient) return;

            try
            {
                var deletedPatienId = SelectedPatient.Id;
                IsBusy = true;
                BusyMessage = "Удаление пациента...";
                await _patientsService
                    .DeleteAsync(deletedPatienId)
                    .ConfigureAwait(true);
                Patients.Remove(SelectedPatient);
                SelectedPatient = null;
                _notifier.ShowSuccess("Пациент удален");
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"{GetType().Name}: Ошибка удаления пациента с Id {SelectedPatient?.Id}. Причина: {ex.Message}",
                    ex);

                _notifier.ShowError("Ошибка удаления пациента.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void PatientSearch(object sender)
        {
        }

        public void CancelSearch()
        {
            MessageHelper.Instance.ShowMessageAsync("Cancel");
        }

        private bool CanSearch(object sender)
        {
            if (!(sender is string searchQuery))
            {
                return false;
            }
            return !String.IsNullOrWhiteSpace(searchQuery);
        }

        private async Task OpenSessionsAsync()
        {
            await PageTransitionRequested.InvokeAsync(this,
                new TransitionRequest(PageIds.PatientSessionsPageId, new PatientSessionsPageContext
                {
                    Patient = SelectedPatient
                })).ConfigureAwait(false);
        }


        public void Dispose()
        {
            _patientAddedEventHandler?.Unsubscribe();
            _patientChangedEventHandler?.Unsubscribe();

            _patientAddedEventHandler?.Dispose();
            _patientChangedEventHandler?.Dispose();
        }

        private async Task UpdatePatientsSafeAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка списка пациентов...";

                var patients = await  _patientsService
                    .GetAllAsync()
                    .ConfigureAwait(true);
                Patients = patients != null
                    ? new ObservableCollection<Patient>(patients)
                    : new ObservableCollection<Patient>();
                _isPatientListChanged = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка обновление списка пациентов. Причина: {ex.Message}", ex);
                _notifier.ShowError("Ошибка обновления списка пациентов");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #region IStoryboardViewModel

        
        public Guid PageId { get; set; }

        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            _patientAddedEventHandler.Subscribe();
            _patientChangedEventHandler.Subscribe();
            Task.Factory.StartNew(async () => await UpdatePatientsSafeAsync().ConfigureAwait(false));
            return Task.CompletedTask;
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
            if (_isPatientListChanged)
            {
                Task.Factory.StartNew(async () => await UpdatePatientsSafeAsync().ConfigureAwait(false));
            }
            return Task.CompletedTask;
        }

        public Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync()
        {
            _patientAddedEventHandler.Unsubscribe();
            _patientChangedEventHandler.Unsubscribe();
            return Task.CompletedTask;
        }

        public event Func<TransitionEvent, Task> PageCanceled;

        public event Func<TransitionEvent, Task> PageCompleted;

        public event Func<TransitionEvent, Task> PageBackRequested;

        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion
    }
}
