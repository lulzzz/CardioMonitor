using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.Communication;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;

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

        public PatientViewModel(ILogger logger, IPatientsService patientsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
        }

        public EventHandler MoveBackwardEvent { get; set; }

        public AccessMode AccessMode
        {
            get { return _accessMode; }
            set
            {
                IsSaved = true;
                if (value != _accessMode)
                {
                    _accessMode = value;
                    //magic for changing title
                    Title = "";
                }
            }
        }

        public Patient Patient
        {
            get
            {
                return new Patient
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PatronymicName =  PatronymicName,
                    Id = _id,
                    BirthDate = BirthDate
                };
            }

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
            get { return _lastName; }
            set
            {
                if (value != _lastName)
                {
                    _lastName = value;
                    RisePropertyChanged("LastName");
                    IsSaved = false;
                }
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (value != _firstName)
                {
                    _firstName = value;
                    RisePropertyChanged("FirstName");
                    IsSaved = false;
                }
            }
        }

        public string PatronymicName
        {
            get { return _patronymicName; }
            set
            {
                if (value != _patronymicName)
                {
                    _patronymicName = value;
                    RisePropertyChanged("PatronymicName");
                    IsSaved = false;
                }
            }
        }

        public DateTime? BirthDate
        {
            get { return _birthDate; }
            set 
            {
                    _birthDate = value;
                    RisePropertyChanged("BirthDate");
                    IsSaved = false;

            }
        }

        public string Title
        {
            get
            {
                return (AccessMode.Create == AccessMode)
                    ? Localisation.PatientViewModel_Title_Add
                    : Localisation.PatientViewModel_Title_Edit;
            }
            set
            {
                RisePropertyChanged("Title");
            }
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
            var message = String.Empty;
            try
            {
                switch (AccessMode)
                {
                    case AccessMode.Create:
                        _patientsService.Add(Patient);
                        message = Localisation.PatientViewModel_Patient_Added;
                        break;
                    case AccessMode.Edit:
                        _patientsService.Edit(Patient);
                        message = Localisation.PatientViewModel_Patient_Updated;
                        break;
                }
                IsSaved = true;

                if (PageBackRequested != null)
                {
                    await PageBackRequested.Invoke(this).ConfigureAwait(true);
                }
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(nameof(PatientViewModel),ex);
                message = Localisation.ArgumentNullExceptionMessage;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!String.IsNullOrEmpty(message))
            {
                await MessageHelper.Instance.ShowMessageAsync(message);
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
                var result = await MessageHelper.Instance.ShowMessageAsync("Все несохраненные изменения будут потеряны. Вы уверены?", "Cardio Monitor", MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true);
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
