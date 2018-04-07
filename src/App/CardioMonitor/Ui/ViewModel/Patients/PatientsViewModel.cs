using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.Communication;
using CardioMonitor.Ui.ViewModel.Sessions;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.Logging;

namespace CardioMonitor.Ui.ViewModel.Patients
{
    public class PatientsViewModel : Notifier, IStoryboardPageViewModel
    {
        private readonly IPatientsService _patientsService;
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
            [NotNull] ILogger logger)
        {
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                IsBusy = true;
                BusyMessage = "Удаление пациента...";
                await _patientsService
                    .DeleteAsync(SelectedPatient.Id)
                    .ConfigureAwait(true);
                Patients.Remove(SelectedPatient);
                SelectedPatient = null;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"{GetType().Name}: Ошибка удаления пациента с Id {SelectedPatient?.Id}. Причина: {ex.Message}",
                    ex);

                await MessageHelper.Instance.ShowMessageAsync("Ошибка удаления пациента.").ConfigureAwait(true);
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

            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка обновление списка пациентов. Причина: {ex.Message}", ex);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка обновления списка пациентов").ConfigureAwait(true);
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

        public event Func<object, Task> PageCanceled;

        public event Func<object, Task> PageCompleted;

        public event Func<object, Task> PageBackRequested;

        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion
    }
}
