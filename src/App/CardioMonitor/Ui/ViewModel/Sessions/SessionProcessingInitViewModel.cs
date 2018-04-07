using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using Markeli.Storyboards;
using Markeli.Utils.Logging;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionProcessingInitViewModel : Notifier, IStoryboardPageViewModel, IDataErrorInfo
    {
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
            }

        }
        private short _cyclesCount;

        /// <summary>
        /// Частота движения
        /// </summary>
        public float MovementFrequency
        {
            get => _movementFrequency;
            set
            {
                if (Equals(_movementFrequency, value)) return;
                _movementFrequency = value;
                RisePropertyChanged(nameof(MovementFrequency));
            }

        }
        private float _movementFrequency;

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
            }
        }
        private short _pumpingNumberOfAttemptsOnStartAndFinish;

        /// <summary>
        /// Количество попыток накачики в процессе выполнения сеанса
        /// </summary>
        public short PumpingNumberOfAttemptsOnProcessing
        {
            get => _pumpingNumberOfAttemptsOnStartAndFinish;
            set
            {
                if (value == _pumpingNumberOfAttemptsOnProcessing) return;
                _pumpingNumberOfAttemptsOnProcessing = value;
                RisePropertyChanged(nameof(PumpingNumberOfAttemptsOnProcessing));
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

        public SessionProcessingInitViewModel(
            [NotNull] IPatientsService patientsService,
            [NotNull] ILogger logger)
        {
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
        }

        private async Task StartSessionAsync()
        {
            await PageTransitionRequested.InvokeAsync(this,
                new TransitionRequest(PageIds.SessionProcessingPageId, new SessionProcessingPageConetxt
                {
                    PatientId = SelectedPatient.PatientId,
                    MaxAngleX = MaxAngleX,
                    CyclesCount = CyclesCount,
                    IsAutopumpingEnabled = IsAutopumpingEnabled,
                    MovementFrequency = MovementFrequency
                })).ConfigureAwait(true);
        }

        private async Task InitPageAsync(IStoryboardPageContext context)
        {
            if (!(context is SessionProcessingInitPageContext pageContext)) throw new ArgumentException("Incorrect argument");
            if (_previouslySelectedPatientIdFromContext == pageContext.PatientId && Patients != null) return;

            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка информации о пациентах...";
                Patients = await Task.Factory.StartNew(_patientsService.GetPatientNames).ConfigureAwait(true);

                if (pageContext.PatientId.HasValue)
                {
                    SelectedPatient = Patients.FirstOrDefault(x => x.PatientId == pageContext.PatientId.Value);
                }

                _previouslySelectedPatientIdFromContext = pageContext.PatientId;
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
                if (columnName == nameof(SelectedPatient) && SelectedPatient == null)
                {
                    IsValid = false;
                    return "Нужно выбрать пациента";
                }
                IsValid = true;
                return null;
            }
        }

        public string Error => String.Empty;

        public bool IsValid
        {
            get => _isValid;
            set
            {
                var oldValod = _isValid;
                _isValid = value;

                if (oldValod != value)
                {
                    RisePropertyChanged(nameof(IsValid));
                    RisePropertyChanged(nameof(StartSessionCommand));
                }
            }
        }

        #endregion
    }
}