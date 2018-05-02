using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Events.Patients;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.Communication;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;

namespace CardioMonitor.Ui.ViewModel.Patients
{
    public class PatientViewModel : Notifier, IStoryboardPageViewModel, IDataErrorInfo
    {
        private readonly ILogger _logger;
        private readonly IPatientsService _patientsService;
        private AccessMode _accessMode;
        private string _lastName;
        private string _firstName;
        private string _patronymicName;
        private int _id;
        private DateTime? _birthDate;
        private ICommand _saveCommand;
        private readonly IEventBus _eventBus;

        private readonly ToastNotifications.Notifier _notifier;

        public PatientViewModel(
            ILogger logger, 
            IPatientsService patientsService, [NotNull] IEventBus eventBus, 
            [NotNull] ToastNotifications.Notifier notifier)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        public EventHandler MoveBackwardEvent { get; set; }

        public AccessMode AccessMode
        {
            get => _accessMode;
            set
            {
                IsSaved = true;
                if (value == _accessMode) return;

                _accessMode = value;
                //magic for changing title
                Title = "";
            }
        }

        public Patient Patient
        {
            get => new Patient
            {
                FirstName = FirstName,
                LastName = LastName,
                PatronymicName =  PatronymicName,
                Id = _id,
                BirthDate = BirthDate
            };

            set
            {
                _id = value.Id;
                LastName = value.LastName;
                FirstName = value.FirstName;
                PatronymicName = value.PatronymicName;
                BirthDate = value.BirthDate;
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (value != _lastName)
                {
                    _lastName = value;
                    RisePropertyChanged(nameof(LastName));
                    IsSaved = false;
                    RisePropertyChanged(nameof(SaveCommand));
                }
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (value != _firstName)
                {
                    _firstName = value;
                    RisePropertyChanged(nameof(FirstName));
                    IsSaved = false;
                    RisePropertyChanged(nameof(SaveCommand));
                }
            }
        }

        public string PatronymicName
        {
            get => _patronymicName;
            set
            {
                if (value == _patronymicName) return;
                _patronymicName = value;
                RisePropertyChanged(nameof(PatronymicName));
                IsSaved = false;
                RisePropertyChanged(nameof(SaveCommand));
            }
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set 
            {
                    _birthDate = value;
                    RisePropertyChanged(nameof(BirthDate));
                IsSaved = false;
                RisePropertyChanged(nameof(SaveCommand));

            }
        }

        public string Title
        {
            get => (AccessMode.Create == AccessMode)
                ? Localisation.PatientViewModel_Title_Add
                : Localisation.PatientViewModel_Title_Edit;
            set => RisePropertyChanged(nameof(Title));
        }

        public bool IsSaved { get; set; }


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
                    CanExecuteDelegate = x => !IsSaved && IsValid,
                    ExecuteDelegate = async x => await Save().ConfigureAwait(true)
                });
            }
        }

        private async Task Save()
        {
            var operationName = String.Empty;
            try
            {
                IsBusy = true;
                switch (AccessMode)
                {
                    case AccessMode.Create:
                        operationName = "создании нового";
                        BusyMessage = "Создание нового пользователя...";
                        await _patientsService
                            .AddAsync(Patient)
                            .ConfigureAwait(false);
                        await _eventBus
                            .PublishAsync(new PatientAddedEvent())
                            .ConfigureAwait(true);
                        _notifier.ShowSuccess("Добавлен новый пользователь");
                        break;
                    case AccessMode.Edit:
                        operationName = "редактировании";
                        BusyMessage = "Редактирование нового пользователя...";
                        await _patientsService
                            .EditAsync(Patient)
                            .ConfigureAwait(false);
                        await _eventBus
                            .PublishAsync(new PatientChangedEvent(Patient.Id))
                            .ConfigureAwait(true);
                        _notifier.ShowSuccess("Данные отредактированы");
                        break;
                }

                IsSaved = true;
                await PageCompleted.InvokeAsync(new TransitionEvent(this)).ConfigureAwait(true);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(
                    $"{GetType().Name}: Ошибка при {operationName} пациента. Причина: {Localisation.ArgumentNullExceptionMessage}",
                    ex);
                _notifier.ShowError(Localisation.ArgumentNullExceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка при {operationName} пациента. Причина: {ex.Message}", ex);
                _notifier.ShowError($"Ошибка при {operationName}");
            }
            finally
            {
                IsBusy = false;
            }

        }


        
        public void Dispose()
        {
        }

        #region StoryBoardPageViewModel


        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            if (!(context is PatientPageContext pageContext)) throw new ArgumentException("Incorrect type of arguments");

            Patient = pageContext.Patient;
            AccessMode = pageContext.Mode;

            return Task.CompletedTask;
        }

        public async Task<bool> CanLeaveAsync()
        {
            if (!IsSaved)
            {
                var result = await MessageHelper.Instance
                    .ShowMessageAsync("Данные будут потеряны. Вы уверены?", "Cardio Monitor", MessageDialogStyle.AffirmativeAndNegative)
                    .ConfigureAwait(true);
                return result == MessageDialogResult.Affirmative;
            }
            return true;
        }

        public Task LeaveAsync()
        {
            return Task.CompletedTask;
        }

        public Task ReturnAsync(IStoryboardPageContext context)
        {
            //validation
            var temp = this[String.Empty];
            return Task.CompletedTask;
        }

        public async Task<bool> CanCloseAsync()
        {
            if (!IsSaved)
            {
                var result = await MessageHelper.Instance.ShowMessageAsync(
                    "Все несохраненные изменения будут потеряны. Вы уверены?", 
                    "Cardio Monitor", 
                    MessageDialogStyle.AffirmativeAndNegative)
                    .ConfigureAwait(true);
                return result == MessageDialogResult.Affirmative;
            }

            return true;
        }

        public Task CloseAsync()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            PatronymicName = String.Empty;
            BirthDate = null;
            return Task.CompletedTask;
        }

        public event Func<TransitionEvent, Task> PageCanceled;

        public event Func<TransitionEvent, Task> PageCompleted;

        public event Func<TransitionEvent, Task> PageBackRequested;

        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion

        #region DataErrorInfo

        public string this[string columnName]
        {
            get
            {
                if (String.IsNullOrEmpty(columnName) || String.Equals(columnName, nameof(LastName)))
                {
                    if (String.IsNullOrEmpty(LastName))
                    {
                        return "Необходимо указать фамилию пациента";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || String.Equals(columnName, nameof(FirstName)))
                {
                    if (String.IsNullOrEmpty(FirstName))
                    {
                        return "Необходимо указать имя пациента";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || String.Equals(columnName, nameof(PatronymicName)))
                {
                    if (String.IsNullOrEmpty(PatronymicName))
                    {
                        return "Необходимо указать отчетсво пациента";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || String.Equals(columnName, nameof(BirthDate)))
                {
                    if (!BirthDate.HasValue)
                    {
                        return "Необходимо указать дату рождения пациента";
                    }
                }
                
                return String.Empty;
            }
        }

        public bool IsValid => String.IsNullOrEmpty(this[String.Empty]);
 
        public string Error  =>String.Empty;
        


        #endregion

    }
}
