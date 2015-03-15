using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Core.Repository;
using CardioMonitor.Patients;
using CardioMonitor.Patients.Session;
using CardioMonitor.Patients.Sessions;
using CardioMonitor.Patients.TreatmentData;
using CardioMonitor.Patients.Treatments;
using CardioMonitor.Settings;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor
{

    public enum ViewIndex
    {
        NotSelected = -1,
        PatientsView = 0,
        TreatmentsView = 1,
        SessionsView = 2,
        SessionView = 3,
        TreatmentDataView = 4,
        SessionDataView = 5,
        PatientView = 6
    }

    public class MainWindowViewModel : Notifier
    {
        private ICommand _moveBackwardComand;
        private int _mainTCSelectedIndex;
        private int _mainTCPreviosSelectedIndex;
        private PatientsViewModel _patientsViewModel;
        private PatientViewModel _patientViewModel;
        private TreatmentsViewModel _treatmentsViewModel;
        private SessionsViewModel _sessionsViewModel;
        private SessionViewModel _sessionViewModel;
        private SessionDataViewModel _sessionDataViewModel;
        private TreatmentDataViewModel _treatmentDataViewModel;
        private SettingsViewModel _settingsViewModel;

        public ICommand MoveBackwardCommand
        {
            get
            {
                return _moveBackwardComand ??
                       (_moveBackwardComand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => true,
                               ExecuteDelegate = x => MoveBackwardView(x)
                           });
            }
        }

        public int MainTCSelectedIndex
        {
            get { return _mainTCSelectedIndex; }
            set
            {
                if (value != _mainTCSelectedIndex)
                {
                    _mainTCPreviosSelectedIndex = _mainTCSelectedIndex;
                    _mainTCSelectedIndex = value;
                    RisePropertyChanged("MainTCSelectedIndex");
                }
            }
        }

        public PatientsViewModel PatientsViewModel
        {
            get { return _patientsViewModel; }
            set
            {
                if (value != _patientsViewModel)
                {
                    _patientsViewModel = value;
                    RisePropertyChanged("PatientsViewModel");
                }
            }
        }

        public PatientViewModel PatientViewModel
        {
            get { return _patientViewModel; }
            set
            {
                if (value != _patientViewModel)
                {
                    _patientViewModel = value;
                    RisePropertyChanged("PatientViewModel");
                }
            }
        }

        public TreatmentsViewModel TreatmentsViewModel
        {
            get { return _treatmentsViewModel; }
            set
            {
                if (value != _treatmentsViewModel)
                {
                    _treatmentsViewModel = value;
                    RisePropertyChanged("TreatmentsViewModel");
                }
            }
        }


        public SessionsViewModel SessionsViewModel
        {
            get { return _sessionsViewModel; }
            set
            {
                if (value != _sessionsViewModel)
                {
                    _sessionsViewModel = value;
                    RisePropertyChanged("SessionsViewModel");
                }
            }
        }

        public SessionViewModel SessionViewModel
        {
            get { return _sessionViewModel; }
            set
            {
                if (value != _sessionViewModel)
                {
                    _sessionViewModel = value;
                    RisePropertyChanged("SessionViewModel");
                }
            }
        }

        public SessionDataViewModel SessionDataViewModel
        {
            get { return _sessionDataViewModel; }
            set
            {
                if (value != _sessionDataViewModel)
                {
                    _sessionDataViewModel = value;
                    RisePropertyChanged("SessionDataViewModel");
                }
            }
        }

        public TreatmentDataViewModel TreatmentDataViewModel
        {
            get { return _treatmentDataViewModel; }
            set
            {
                if (value != _treatmentDataViewModel)
                {
                    _treatmentDataViewModel = value;
                    RisePropertyChanged("TreatmentDataViewModel");
                }
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get { return _settingsViewModel; }
            set
            {
                if (value != _settingsViewModel)
                {
                    _settingsViewModel = value;
                    RisePropertyChanged("SettingsViewModel");
                }
            }
        }

        public MainWindowViewModel()
        {
            PatientsViewModel = new PatientsViewModel
            {
                OpenPatienEvent = OpetPatientTreatmentsHanlder,
                AddEditPatient = AddEditPatientHanlder,
                OpenSessionsHandler = StartOrContinueTreatmentSession,
                ShowTreatmentResults = ShowTreatmentResults,
                OpenSessionHandler = LoadSession
            };
            PatientViewModel = new PatientViewModel();
            TreatmentsViewModel = new TreatmentsViewModel
            {
                OpenSessionsEvent = StartOrContinueTreatmentSession,
                ShowResultsEvent = ShowTreatmentResults
            };
            SessionsViewModel = new SessionsViewModel
            {
                StartSessionEvent = StartSession,
                ShowResultsEvent = ShowSessionResults
            };
            SessionViewModel = new SessionViewModel();
            SessionDataViewModel = new SessionDataViewModel();
            TreatmentDataViewModel = new TreatmentDataViewModel();
            SettingsViewModel = new SettingsViewModel();
        }

        public void UpdatePatiens()
        {
            try
            {
                PatientsViewModel.Patients = Repository.Instance.GetPatients();
            }
            catch (Exception ex)
            {
                MessageHelper.Instance.ShowMessageAsync(ex.Message);
            }
        }

        private async void MoveBackwardView(object sender)
        {
            int index;
            if (!int.TryParse(sender.ToString(), out index)) { return; }
            var viewIndex = (ViewIndex)index;
            switch (viewIndex)
            {
                case ViewIndex.TreatmentsView:
                    _treatmentsViewModel.Clear();
                    UpdatePatiens();
                    MainTCSelectedIndex = (int)ViewIndex.PatientsView;
                    break;
                case ViewIndex.SessionsView:
                    _sessionsViewModel.Clear();
                    UpdatePatiens();
                    MainTCSelectedIndex = (int)ViewIndex.PatientsView;
                    //MainTCSelectedIndex = (int)ViewIndex.TreatmentsView;
                    break;
                case ViewIndex.SessionView:
                    if (SessionStatus.InProgress == _sessionViewModel.Status)
                    {
                        var result = await MessageHelper.Instance.ShowMessageAsync("Все несохраненые изменения будут потеряны. Продолжить?", style: MessageDialogStyle.AffirmativeAndNegative);
                        if (MessageDialogResult.Negative == result) { return; }
                    }
                    _sessionViewModel.Clear();
                    UpdateSessionInfosSave();
                    MainTCSelectedIndex = (int)ViewIndex.SessionsView;
                    break;
                case ViewIndex.SessionDataView:
                    if (_mainTCPreviosSelectedIndex == (int) ViewIndex.PatientsView)
                    {
                        MainTCSelectedIndex = (int) ViewIndex.PatientsView;
                        UpdatePatiens();
                    }
                    else
                    {
                        MainTCSelectedIndex = (int)ViewIndex.SessionsView;
                        UpdateSessionInfosSave();
                    }
                    break;
                case ViewIndex.TreatmentDataView:
                    //MainTCSelectedIndex = (int)ViewIndex.TreatmentsView;
                    UpdatePatiens();
                    MainTCSelectedIndex = (int)ViewIndex.PatientsView;
                    break;
                case ViewIndex.PatientView:
                    if (!_patientViewModel.IsSaved)
                    {
                        var result = await MessageHelper.Instance.ShowMessageAsync("Все несохраненые изменения будут потеряны. Продолжить?", style: MessageDialogStyle.AffirmativeAndNegative);
                        if (MessageDialogResult.Negative == result) { return;}
                    }
                    _patientViewModel.Clear();
                    UpdatePatiens();
                    MainTCSelectedIndex = (int) ViewIndex.PatientsView;
                    break;
                default:
                    UpdatePatiens();
                    MainTCSelectedIndex = (int) ViewIndex.PatientsView;
                    break;
            }
        }

        public async void OpetPatientTreatmentsHanlder(object sender, EventArgs eventArgs)
        {
            var cardioEventArgs = eventArgs as CardioEventArgs;
            if (null == cardioEventArgs) { return;}
            var patient = PatientsViewModel.SelectedPatient;
            TreatmentsViewModel.PatientName = new PatientFullName
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                PatronymicName = patient.PatronymicName
            };
            //some magic
            MainTCSelectedIndex = (int) ViewIndex.TreatmentsView;
        }

        public void AddEditPatientHanlder(object sender, EventArgs args)
        {
            try
            {
                var patientEventArgs = args as PatientEventArgs;
                if (null == patientEventArgs)
                {
                    return;
                }
                _patientViewModel.Patient = patientEventArgs.Patient;
                _patientViewModel.AccessMode = patientEventArgs.Mode;
                MainTCSelectedIndex = (int)ViewIndex.PatientView;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public async void StartOrContinueTreatmentSession(object sender, EventArgs args)
        {
            //TODO Old realisation
            var isSuccessfull = false;
            var patient = PatientsViewModel.SelectedPatient;
            try
            {

                var treamtnets = Repository.Instance.GetTreatments(patient.Id);
                var treatment = treamtnets.FirstOrDefault();
                if (null == treatment)
                {
                    Repository.Instance.AddTreatment(new Treatment { StartDate = DateTime.Now, PatientId = patient.Id });
                    treatment = Repository.Instance.GetTreatments(patient.Id).FirstOrDefault();
                    if (null == treatment)
                    {
                        await MessageHelper.Instance.ShowMessageAsync("Не удалось получить список ");
                        return;
                    }
                }
                SessionsViewModel.PatientName = new PatientFullName
                {
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    PatronymicName = patient.PatronymicName
                };
                SessionsViewModel.Treatment = treatment;
                SessionViewModel.Treatment = treatment;
                UpdateSessionInfos();
                MainTCSelectedIndex = (int)ViewIndex.SessionsView;
                isSuccessfull = true;
            }
            catch (Exception ex)
            {
                isSuccessfull = false;
            }
            if (!isSuccessfull)
            {
                await MessageHelper.Instance.ShowMessageAsync("Не удалось получить список сеансов.");
            }
        }

        private void UpdateSessionInfos()
        {
            var sessions = Repository.Instance.GetSessionInfos(SessionsViewModel.Treatment.Id);
            SessionsViewModel.SessionInfos = sessions;
        }

        private void UpdateSessionInfosSave()
        {
            try
            {
                UpdateSessionInfos();
            }
            catch (Exception)
            {
                MessageHelper.Instance.ShowMessageAsync("Не удалось получить список сеансов.");
            }
        }

        //temporary not  used
        public void ShowTreatmentResults(object sender, EventArgs args)
        {
            //var treatmentId = _treatmentsViewModel.SelectedTreatment.Id;
            //getting result
            var session = new SessionModel {DateTime = new DateTime()};
            
            var statisticBuilder = new TreatmentStatisticBuilder();
            TreatmentDataViewModel.Statistic = statisticBuilder.Build(new Session[]
            {
                session.Session, session.Session, session.Session, session.Session, session.Session, session.Session, session.Session,session.Session ,session.Session ,session.Session
            });
            TreatmentDataViewModel.PatientName = new PatientFullName
            {
                LastName = PatientsViewModel.SelectedPatient.LastName,
                FirstName = PatientsViewModel.SelectedPatient.FirstName,
                PatronymicName = PatientsViewModel.SelectedPatient.PatronymicName,
            };
            TreatmentDataViewModel.StartDate = session.DateTime;
            MainTCSelectedIndex = (int) ViewIndex.TreatmentDataView;
        }

        private void StartSession(object sender, EventArgs args)
        {
            SessionViewModel.Patient = PatientsViewModel.SelectedPatient;
            
            MainTCSelectedIndex = (int) ViewIndex.SessionView;
        }

        private async void ShowSessionResults(object sender, EventArgs args)
        {
            var isShowingSuccessfull = false;
            try
            {

                var session = Repository.Instance.GetSession(SessionsViewModel.SelectedSessionInfo.Id);
                SessionDataViewModel.Session = new SessionModel {Session = session};
                SessionDataViewModel.Patient = PatientsViewModel.SelectedPatient;
                MainTCSelectedIndex = (int)ViewIndex.SessionDataView;
                isShowingSuccessfull = true;
            }
            catch (Exception)
            {
                isShowingSuccessfull = false;
            }
            if (!isShowingSuccessfull)
            {
                await MessageHelper.Instance.ShowMessageAsync("Не удалость получить данные за сеанс.");
            }
        }

        private void LoadSession(object sender, EventArgs args)
        {
            var loadDialog = new OpenFileDialog {CheckFileExists = true, Filter = "Сеанс|*.cmsf"};
            var result = loadDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    var container = FileManager.LoadFromFile(loadDialog.FileName);
                    SessionDataViewModel.Session = new SessionModel {Session = container.Session};
                    SessionDataViewModel.Patient = container.Patient;
                    /*{
                        LastName = container.Patient.LastName,
                        FirstName =  container.Patient.FirstName,
                        PatronymicName = container.Patient.PatronymicName
                    };*/
                    MainTCSelectedIndex = (int) ViewIndex.SessionDataView;
                }
                catch (Exception ex)
                {
                    MessageHelper.Instance.ShowMessageAsync("Не удалось открыть файл");
                }
            }
        }
    }

  
}
