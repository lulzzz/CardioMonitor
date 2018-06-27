using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.WpfCommon.Base;
using CardioMonitor.Resources;
using JetBrains.Annotations;
using Markeli.Storyboards;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;
using ArgumentException = System.ArgumentException;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionDataViewModel : Notifier, IStoryboardPageViewModel
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IFilesManager _filesRepository;
        private PatientFullName _patientName;
        private Patient _patient;
        private ICommand _saveCommand;
        private readonly ISessionsService _sessionsService;
        private readonly IPatientsService _patientsService;
        [NotNull]
        private readonly ToastNotifications.Notifier _notifier;

        private IReadOnlyList<CycleData> _patientParamsPerCycles;

        private string _busyMessage;

        private bool _isReadOnly;

        private SessionStatus _sessionStatus;
        private DateTime _sessionTimestampUtc;
        private int _selectedCycleTab;
        #endregion

        public SessionDataViewModel(
            ISessionsService sessionsService, 
            IPatientsService patientsService,
            ILogger logger,
            IFilesManager filesRepository, 
            [NotNull] ToastNotifications.Notifier notifier)
        {
            _sessionsService = sessionsService;
            _patientsService = patientsService;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
            _notifier = notifier;

            IsReadOnly = true;
        }

        #region Properties

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

        public IReadOnlyList<Patient> Patients => Patient != null 
            ? new List<Patient> { Patient }
            : new List<Patient>();

     
        /// <summary>
        /// Показатели пациента с разделением по циклам
        /// </summary>
        public IReadOnlyList<CycleData> PatientParamsPerCycles
        {
            get => _patientParamsPerCycles;
            set
            {
                _patientParamsPerCycles = value;
                RisePropertyChanged(nameof(PatientParamsPerCycles));
            }
        }

        /// <summary>
        /// Признак доступности данных только для чтения
        /// </summary>
        //todo Непонятно, зачем он тут нужен
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                RisePropertyChanged(nameof(IsReadOnly));
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

        public SessionStatus SessionStatus
        {
            get => _sessionStatus;
            set
            {
                _sessionStatus = value;
                RisePropertyChanged(nameof(SessionStatus));
            }
        }

        public DateTime SessionTimestampUtc
        {
            get => _sessionTimestampUtc;
            set
            {
                _sessionTimestampUtc = value;
                RisePropertyChanged(nameof(SessionTimestampUtc));
            }
        }
        
        public int SelectedCycleTab
        {
            get => _selectedCycleTab;
            set
            {
                _selectedCycleTab = value;
                RisePropertyChanged(nameof(SelectedCycleTab));
            }
        }
        #endregion

        #region Commands

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


        #endregion

        public async void SaveToFile()
        {
            var saveFileDialog = new SaveFileDialog {Filter = Localisation.FileRepository_SeansFileFilter};
            var dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult != DialogResult.OK) return;

            try
            {
                IsBusy = true;
                BusyMessage = "Сохранение в файл...";
           //     _filesRepository.SaveToFile(Patient, Session.Session, saveFileDialog.FileName);
                await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionDataViewModel_FileSaved);
                _notifier.ShowSuccess("Сеанс успешно сохранен");
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка сохранения сессии в файл. Причина: {ex.Message}", ex);
                _notifier.ShowError("Ошибка сохранения сеанса в файл");
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
                if (session?.Cycles == null) throw new ArgumentException(
                    $"Сессия с Id {sessionId} не получена или не получены информация о повторениях");

                var patientParamsPerCycles = new List<CycleData>(session.Cycles.Count);
                foreach (var sessionCycle in session.Cycles)
                {
                    var patientParamsPerCycle = sessionCycle.PatientParams.Select(x =>
                    {
                        var checkPoint = new CheckPointParams((short) sessionCycle.CycleNumber, (short) x.Iteraton,
                            x.InclinationAngle)
                        {
                            AverageArterialPressure = x.AverageArterialPressure,
                            DiastolicArterialPressure = x.DiastolicArterialPressure,
                            HeartRate = x.HeartRate,
                            RespirationRate = x.RepsirationRate,
                            Spo2 = x.Spo2,
                            SystolicArterialPressure = x.SystolicArterialPressure,
                            
                        };
                        return checkPoint;
                    });
                    
                    var cycleData = new CycleData
                    {
                        CycleNumber = (short) sessionCycle.CycleNumber,
                        CycleParams = new ObservableCollection<CheckPointParams>(patientParamsPerCycle)
                    };
                    patientParamsPerCycles.Add(cycleData);
                }

                SessionStatus = session.Status;
                SessionTimestampUtc = session.TimestampUtc;
                SelectedCycleTab = 0;
                
                PatientParamsPerCycles = patientParamsPerCycles;
                
                var patient =  await _patientsService
                    .GetPatientAsync(patientId)
                    .ConfigureAwait(true);
                Patient = patient;
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка загрузки сеанса. Причина: {ex.Message}", ex);
                _notifier.ShowError("Ошибка загрузки сеанса");
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
