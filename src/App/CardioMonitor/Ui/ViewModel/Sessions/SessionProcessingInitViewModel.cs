using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.EventHandlers.Patients;
using CardioMonitor.Events.Patients;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using Markeli.Storyboards;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.Logging;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionProcessingInitViewModel : Notifier, IStoryboardPageViewModel, IDataErrorInfo
    {
        private readonly float Tolerance = 1e-4f;

        #region Events
        
        private readonly IEventBus _eventBus;
        private readonly PatientAddedEventHandler _patientAddedEventHandler;
        private readonly PatientChangedEventHandler _patientChangedEventHandler;
        private readonly PatientDeletedEventHandler _patientDeletedEventHandler;

        private bool _isPatienListChanged;

        #endregion

        private readonly ISessionParamsValidator _sessionParamsValidator;
        private readonly IPatientsService _patientsService;
        private readonly ILogger _logger;
        private ICommand _startCommand;
        private bool _isValid;

        private int? _previouslySelectedPatientIdFromContext;

        public IReadOnlyList<PatientFullName> Patients
        {
            get => _patients;
            set
            {
                if (Equals(_patients, value)) return;

                _patients = value;
                RisePropertyChanged(nameof(Patients));
            }
        }
        private IReadOnlyList<PatientFullName> _patients;

        private PatientFullName _selectedPatient;

        public PatientFullName SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (Equals(_selectedPatient, value)) return;

                _selectedPatient = value;
                RisePropertyChanged(nameof(SelectedPatient));
                RisePropertyChanged(nameof(StartSessionCommand));
            }
        }


        /// <summary>
        /// Максимальный угол кровати по оси Х, до которой она будет подниматься
        /// </summary>
        public float MaxAngleX
        {
            get => _maxAngleX;
            set
            {
                if (Equals(_maxAngleX, value)) return;
                _maxAngleX = value;
                RisePropertyChanged(nameof(MaxAngleX));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }
        private float _maxAngleX;

        /// <summary>
        /// Количество циклов (повторений)
        /// </summary>
        public short CyclesCount
        {
            get => _cyclesCount;
            set
            {
                if (Equals(_cyclesCount, value)) return;
                _cyclesCount = value;
                RisePropertyChanged(nameof(CyclesCount));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }
        private short _cyclesCount;

        /// <summary>
        /// Частота движения
        /// </summary>
        public double MovementFrequency
        {
            get => _movementFrequency;
            set
            {
                if (Math.Abs(_movementFrequency - value) < Tolerance) return;
                _movementFrequency = (double)Math.Round((Decimal)value, 3, MidpointRounding.AwayFromZero);
                RisePropertyChanged(nameof(MovementFrequency));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }
        private double _movementFrequency;

        /// <summary>
        /// Частота движения
        /// </summary>
        public bool IsAutopumpingEnabled
        {
            get => _isAutopumpingEnabled;
            set
            {
                if (Equals(_isAutopumpingEnabled, value)) return;
                _isAutopumpingEnabled = value;
                RisePropertyChanged(nameof(IsAutopumpingEnabled));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }
        private bool _isAutopumpingEnabled;

        /// <summary>
        /// Количество попыток накачки при старте и финише
        /// </summary>
        public short PumpingNumberOfAttemptsOnStartAndFinish
        {
            get => _pumpingNumberOfAttemptsOnStartAndFinish;
            set
            {
                if (value == _pumpingNumberOfAttemptsOnStartAndFinish) return;
                _pumpingNumberOfAttemptsOnStartAndFinish = value;
                RisePropertyChanged(nameof(PumpingNumberOfAttemptsOnStartAndFinish));
                RisePropertyChanged(nameof(StartSessionCommand));
            }
        }
        private short _pumpingNumberOfAttemptsOnStartAndFinish;

        /// <summary>
        /// Количество попыток накачики в процессе выполнения сеанса
        /// </summary>
        public short PumpingNumberOfAttemptsOnProcessing
        {
            get => _pumpingNumberOfAttemptsOnProcessing;
            set
            {
                if (value == _pumpingNumberOfAttemptsOnProcessing) return;
                _pumpingNumberOfAttemptsOnProcessing = value;
                RisePropertyChanged(nameof(PumpingNumberOfAttemptsOnProcessing));
                RisePropertyChanged(nameof(StartSessionCommand));
            }
        }
        private short _pumpingNumberOfAttemptsOnProcessing;

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


        public ICommand StartSessionCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => IsValid,
                    ExecuteDelegate = async x => await StartSessionAsync().ConfigureAwait(true)
                });
            }
        }

        #region DefaultValues


        public short MinCyclesCount => SessionParamsConstants.MinCyclesCount;

        public short MaxCyclesCount => SessionParamsConstants.MaxCyclesCount;

        
        public float MinValueMaxXAngle => SessionParamsConstants.MinValueMaxXAngle;

        public float MaxValueMaxXAngle => SessionParamsConstants.MaxValueMaxXAngle;

        public float MaxXAngleStep => SessionParamsConstants.MaxXAngleStep;


        public double MinMovementFrequency => SessionParamsConstants.MinMovementFrequency;

        public double MaxMovementFrequency => SessionParamsConstants.MaxMovementFrequency;

        public double MovementFrequencyStep => SessionParamsConstants.MovementFrequencyStep;


        public short MinPumpingNumberOfAttemptsOnStartAndFinish => SessionParamsConstants.MinPumpingNumberOfAttemptsOnStartAndFinish;

        public short MaxPumpingNumberOfAttemptsOnStartAndFinish => SessionParamsConstants.MaxPumpingNumberOfAttemptsOnStartAndFinish;

        public short MinPumpingNumberOfAttemptsOnProcessing => SessionParamsConstants.MinPumpingNumberOfAttemptsOnProcessing;

        public short MaxPumpingNumberOfAttemptsOnProcessing => SessionParamsConstants.MaxPumpingNumberOfAttemptsOnProcessing;

        #endregion

        public SessionProcessingInitViewModel(
            [NotNull] IPatientsService patientsService,
            [NotNull] ILogger logger,
            [NotNull] ISessionParamsValidator sessionParamsValidator,
            [NotNull] IEventBus eventBus,
            [NotNull] PatientAddedEventHandler patientAddedEventHandler,
            [NotNull] PatientChangedEventHandler patientChangedEventHandler,
            [NotNull] PatientDeletedEventHandler patientDeletedEvent)
        {
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionParamsValidator = sessionParamsValidator ?? throw new ArgumentNullException(nameof(sessionParamsValidator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _patientAddedEventHandler = patientAddedEventHandler ?? throw new ArgumentNullException(nameof(patientAddedEventHandler));
            _patientChangedEventHandler = patientChangedEventHandler ?? throw new ArgumentNullException(nameof(patientChangedEventHandler));
            _patientDeletedEventHandler = patientDeletedEvent ?? throw new ArgumentNullException(nameof(patientDeletedEvent));

            _patientAddedEventHandler.PatientAdded += (sender, args) => _isPatienListChanged = true;
            _patientChangedEventHandler.PatientChanged += (sender, args) => _isPatienListChanged = true;
            _patientDeletedEventHandler.PatientDeleted += (sender, args) => _isPatienListChanged = true;

            _isPatienListChanged = true;
        }

        public void Dispose()
        {
            _patientAddedEventHandler?.Unsubscribe();
            _patientChangedEventHandler?.Unsubscribe();
            _patientDeletedEventHandler?.Unsubscribe();

            _patientAddedEventHandler?.Dispose();
            _patientChangedEventHandler?.Dispose();
            _patientDeletedEventHandler?.Dispose();
        }

        private async Task StartSessionAsync()
        {
            await PageTransitionRequested.InvokeAsync(this,
                new TransitionRequest(PageIds.SessionProcessingPageId, new SessionProcessingPageConext
                {
                    PatientId = SelectedPatient.PatientId,
                    MaxAngleX = MaxAngleX,
                    CyclesCount = CyclesCount,
                    IsAutopumpingEnabled = IsAutopumpingEnabled,
                    MovementFrequency = (float)MovementFrequency,
                    PumpingNumberOfAttemptsOnProcessing = PumpingNumberOfAttemptsOnProcessing,
                    PumpingNumberOfAttemptsOnStartAndFinish = PumpingNumberOfAttemptsOnStartAndFinish
                })).ConfigureAwait(true);
        }

        private Task InitPageAsync(IStoryboardPageContext context)
        {
            if (!(context is SessionProcessingInitPageContext pageContext)) throw new ArgumentException("Incorrect argument");
            SetDefaultValues();
            if (_previouslySelectedPatientIdFromContext == pageContext.PatientId 
                && Patients != null
                && !_isPatienListChanged) return Task.CompletedTask;
            var temp = this[String.Empty];
            return LoadPatientsAsync(pageContext.PatientId);
        }

        private void SetDefaultValues()
        {
            SelectedPatient = null;
            MaxAngleX = SessionParamsConstants.DefaultMaxXAngle;
            CyclesCount = SessionParamsConstants.DefaultCyclesCount;
            MovementFrequency = SessionParamsConstants.DefaultMovementFrequency;
            PumpingNumberOfAttemptsOnProcessing = SessionParamsConstants.DefaultPumpingNumberOfAttemptsOnProcessing;
            PumpingNumberOfAttemptsOnStartAndFinish =
                SessionParamsConstants.DefaultPumpingNumberOfAttemptsOnStartAndFinish;
            IsAutopumpingEnabled = true;
        }

        private async Task LoadPatientsAsync(int? selectedPatientId = null)
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка информации о пациентах...";
                var temp = await _patientsService.GetPatientNamesAsync().ConfigureAwait(true);
                Patients = new List<PatientFullName>(temp);
                if (selectedPatientId.HasValue)
                {
                    SelectedPatient = Patients.FirstOrDefault(x => x.PatientId == selectedPatientId.Value);
                }

                _previouslySelectedPatientIdFromContext = selectedPatientId;
                _isPatienListChanged = false;
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка обновления списка пациентов. Причина: {e.Message}", e);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка обновления списка пациентов")
                    .ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #region IStoryboardPageViewModel
        
        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            _patientAddedEventHandler.Subscribe();
            _patientChangedEventHandler.Subscribe();
            _patientDeletedEventHandler.Subscribe();

            Task.Factory.StartNew(async () => await InitPageAsync(context).ConfigureAwait(false)).ConfigureAwait(false);
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
            Task.Factory.StartNew(async () => await InitPageAsync(context).ConfigureAwait(false)).ConfigureAwait(false);
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

        #region Validation

        public string this[string columnName]
        {
            get
            {
                if ((String.IsNullOrEmpty(columnName) || columnName == nameof(SelectedPatient)) && SelectedPatient == null)
                {
                    return "Нужно выбрать пациента";
                }

                if (String.IsNullOrEmpty(columnName) || columnName == nameof(MaxAngleX))
                {
                    var result = _sessionParamsValidator.IsMaxXAngleValid(MaxAngleX);
                    if (!result)
                    {
                        return $"Нужно указать максимальный угол по оси X в диапазоне [{SessionParamsConstants.MinValueMaxXAngle}; " +
                               $"{SessionParamsConstants.MaxValueMaxXAngle}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || columnName == nameof(CyclesCount))
                {
                    var result = _sessionParamsValidator.IsCyclesCountValid(CyclesCount);
                    if (!result)
                    {
                        return $"Нужно указать количество повторений в диапазоне [{SessionParamsConstants.MinCyclesCount}; " +
                               $"{SessionParamsConstants.MaxCyclesCount}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || columnName == nameof(MovementFrequency))
                {
                    var result = _sessionParamsValidator.IsMovementFrequencyValid((float)MovementFrequency);
                    if (!result)
                    {
                        return $"Нужно указать частоту в диапазоне [{SessionParamsConstants.MinMovementFrequency}; " +
                               $"{SessionParamsConstants.MaxMovementFrequency}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || IsAutopumpingEnabled && columnName == nameof(PumpingNumberOfAttemptsOnStartAndFinish))
                {
                    var result = _sessionParamsValidator.IsPumpingNumberOfAttemptsOnStartAndFinishValid(PumpingNumberOfAttemptsOnStartAndFinish);
                    if (!result)
                    {
                        return $"Нужно указать число в диапазоне [{SessionParamsConstants.MinPumpingNumberOfAttemptsOnStartAndFinish}; " +
                               $"{SessionParamsConstants.MaxPumpingNumberOfAttemptsOnStartAndFinish}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || IsAutopumpingEnabled && columnName == nameof(PumpingNumberOfAttemptsOnProcessing))
                {
                    var result = _sessionParamsValidator.IsPumpingNumberOfAttemptsOnProcessing(PumpingNumberOfAttemptsOnProcessing);
                    if (!result)
                    {
                        return $"Нужно указать число в диапазоне [{SessionParamsConstants.MinPumpingNumberOfAttemptsOnProcessing}; " +
                               $"{SessionParamsConstants.MaxPumpingNumberOfAttemptsOnProcessing}]";
                    }
                }
                return null;
            }
        }

        public string Error => String.Empty;

        public bool IsValid => String.IsNullOrEmpty(this[String.Empty]);

        #endregion
    }
}