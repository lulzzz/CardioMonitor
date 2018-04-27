using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Files;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using Markeli.Storyboards;
using Markeli.Utils.Logging;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    //todo view later
    public class SessionDataViewModel : Notifier, IStoryboardPageViewModel
    {
        private readonly ILogger _logger;
        private readonly IFilesManager _filesRepository;
        private PatientFullName _patientName;
        private Patient _patient;
        private SessionModel _session;
        private ICommand _saveCommand;
        private readonly ISessionsService _sessionsService;
        private readonly IPatientsService _patientsService;

        public SessionDataViewModel(
            ISessionsService sessionsService, 
            IPatientsService patientsService,
            ILogger logger,
            IFilesManager filesRepository)
        {
            _sessionsService = sessionsService;
            _patientsService = patientsService;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));

            IsReadOnly = true;
        }

        public PatientFullName PatientName
        {
            get => _patientName;
            set
            {
                if (value == _patientName) return;
                _patientName = value;
                RisePropertyChanged(nameof(PatientName));
            }
        }

        public Patient Patient
        {
            get => _patient;
            set
            {
                if (value == _patient) return;

                _patient = value;

                PatientName = _patient != null
                    ? new PatientFullName
                    {
                        LastName = _patient.LastName,
                        FirstName = _patient.FirstName,
                        PatronymicName = _patient.PatronymicName,
                    }
                    : new PatientFullName();
                RisePropertyChanged(nameof(Patient));
                RisePropertyChanged(nameof(Patients));
            }
        }

        public ObservableCollection<Patient> Patients => Patient != null 
            ? new ObservableCollection<Patient> { Patient }
            : new ObservableCollection<Patient>();

        public SessionModel Session
        {
            get => _session;
            set
            {
                if (Equals(value, _session)) return;
                _session = value;
                RisePropertyChanged(nameof(Session));
            }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                RisePropertyChanged(nameof(IsReadOnly));
            }
        }
        private bool _isReadOnly;

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

        public async void SaveToFile()
        {
            var saveFileDialog = new SaveFileDialog {Filter = Localisation.FileRepository_SeansFileFilter};
            var dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult != DialogResult.OK) return;

            try
            {
                IsBusy = true;
                BusyMessage = "Сохранение в файл...";
                _filesRepository.SaveToFile(Patient, Session.Session, saveFileDialog.FileName);
                await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionDataViewModel_FileSaved);
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка сохранения сессии в файл. Причина: {ex.Message}", ex);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка сохранения сессии в файл").ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void LoadFromFile()
        {
            //todo what?
            await MessageHelper.Instance.ShowMessageAsync("Opened!").ConfigureAwait(true);
        }

        public void Clear()
        {
            PatientName = null;
            Session = null;
            Patient = null;
        }

        public void Dispose()
        {
        }

        private async Task LoadSessionInfoAsync(int sessionId, int patientId)
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка информации о сеансе...";
                var session =  await _sessionsService
                    .GetAsync(sessionId)
                    .ConfigureAwait(true);
                Session = new SessionModel {Session = session};
                var patient =  await _patientsService
                    .GetPatientAsync(patientId)
                    .ConfigureAwait(true);
                Patient = patient;
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка загрузки сессии. Причина: {ex.Message}", ex);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка загрузки сессии").ConfigureAwait(true);
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
            if (!(context is SessionDataViewingPageContext pageContext)) throw new ArgumentException("Incorrect type of arguments");

            if (String.IsNullOrEmpty(pageContext.FileFullPath))
            {
                Task.Factory.StartNew(
                    async () => await LoadSessionInfoAsync(pageContext.SessionId, pageContext.PatientId));
            }

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

        public event Func<TransitionEvent, Task> PageCanceled;
        public event Func<TransitionEvent, Task> PageCompleted;
        public event Func<TransitionEvent, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion

    }
}
