using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using Markeli.Storyboards;

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

        public SessionDataViewModel(SessionModel session)
        {
            _session = session;
        }

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

                    PatientName = _patient != null
                        ? new PatientFullName
                        {
                            LastName = _patient.LastName,
                            FirstName = _patient.FirstName,
                            PatronymicName = _patient.PatronymicName,
                        }
                        : new PatientFullName();
                    RisePropertyChanged("PatientEntity");
                    RisePropertyChanged("Patients");
                }
            }
        }

        public ObservableCollection<Patient> Patients
        {
            get { return Patient != null 
                    ? new ObservableCollection<Patient> { Patient }
                    : new ObservableCollection<Patient>(); 
            }
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

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RisePropertyChanged("IsReadOnly");
            }
        }
        private bool _isReadOnly;

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
        
        public SessionDataViewModel(
            ILogger logger,
            IFilesManager filesRepository)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (filesRepository == null) throw new ArgumentNullException(nameof(filesRepository));

            _logger = logger;
            _filesRepository = filesRepository;

            IsReadOnly = true;
        }

        public async void SaveToFile()
        {
            var saveFileDialog = new SaveFileDialog {Filter = Localisation.FileRepository_SeansFileFilter};
            var dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {

                var exceptionMessage = String.Empty;
                try
                {
                    _filesRepository.SaveToFile(Patient, Session.Session, saveFileDialog.FileName);
                    await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionDataViewModel_FileSaved);
                }
                catch (ArgumentNullException ex)
                {
                    _logger.LogError(nameof(SessionDataViewModel), ex);
                    exceptionMessage = Localisation.SessionViewModel_SaveSession_ArgumentNullException;
                }
                catch (Exception ex)
                {
                    exceptionMessage = ex.Message;
                }
                if (!String.IsNullOrEmpty(exceptionMessage))
                {
                    await MessageHelper.Instance.ShowMessageAsync(exceptionMessage);
                }
            }
        }

        public async void LoadFromFile()
        {
            //todo what?
            await MessageHelper.Instance.ShowMessageAsync("Opened!");
        }

        public void Clear()
        {
            PatientName = null;
            Session = null;
            Patient = null;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private async Task LoadSessionInfoAsync(int sessionId, int patientId)
        {
            try
            {

                var session = await Task.Factory.StartNew(() => _sessionsService.Get(sessionId));
                Session = new SessionModel { Session = session };
                var patient = await Task.Factory.StartNew(() =>_patientsService.GetPatient(patientId));
                Patient = patient;
            }
            catch (Exception ex)
            {
                await MessageHelper.Instance.ShowMessageAsync(ex.Message);
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

        public event Func<object, Task> PageCanceled;
        public event Func<object, Task> PageCompleted;
        public event Func<object, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion

    }
}
