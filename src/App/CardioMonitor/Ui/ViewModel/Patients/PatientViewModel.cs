using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.Communication;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.Logging;

namespace CardioMonitor.Ui.ViewModel.Patients
{
    public class PatientViewModel : Notifier, IStoryboardPageViewModel
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

        public PatientViewModel(
            ILogger logger, 
            IPatientsService patientsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
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
                }
            }
        }

        public string PatronymicName
        {
            get => _patronymicName;
            set
            {
                if (value != _patronymicName)
                {
                    _patronymicName = value;
                    RisePropertyChanged(nameof(PatronymicName));
                    IsSaved = false;
                }
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

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => !IsSaved,
                    ExecuteDelegate = async x => await Save().ConfigureAwait(true)
                });
            }
        }

        private async Task Save()
        {
            var operationName = String.Empty;
            try
            {
                switch (AccessMode)
                {
                    case AccessMode.Create:
                        _patientsService.Add(Patient);
                        operationName = "создании нового";
                        break;
                    case AccessMode.Edit:
                        _patientsService.Edit(Patient);
                        operationName = "редактировании";
                        break;
                }

                IsSaved = true;

                await PageBackRequested.InvokeAsync(this).ConfigureAwait(true);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка при {operationName} пациента. Причина: {Localisation.ArgumentNullExceptionMessage}", ex);
                await MessageHelper.Instance.ShowMessageAsync(Localisation.ArgumentNullExceptionMessage).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка при {operationName} пациента. Причина: {ex.Message}", ex);
                await MessageHelper.Instance.ShowMessageAsync($"Ошибка при {operationName}").ConfigureAwait(true);
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

        public event Func<object, Task> PageCanceled;

        public event Func<object, Task> PageCompleted;

        public event Func<object, Task> PageBackRequested;

        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion

    }
}
