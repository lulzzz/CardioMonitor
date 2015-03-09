using System;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Patients;
using CardioMonitor.Patients.Session;
using CardioMonitor.Patients.Sessions;
using CardioMonitor.Patients.Treatments;
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
        private PatientsViewModel _patientsViewModel;
        private PatientViewModel _patientViewModel;
        private TreatmentsViewModel _treatmentsViewModel;
        private SessionsViewModel _sessionsViewModel;
        private SessionViewModel _sessionViewModel;

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

        public MainWindowViewModel()
        {
            PatientsViewModel = new PatientsViewModel
            {
                OpenPatienEvent = OpetPatientTreatmentsHanlder,
                AddEditPatient = AddEditPatientHanlder
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
                    MainTCSelectedIndex = (int)ViewIndex.PatientsView;
                    break;
                case ViewIndex.SessionsView:
                    _sessionsViewModel.Clear();
                    MainTCSelectedIndex = (int)ViewIndex.TreatmentsView;
                    break;
                case ViewIndex.SessionView:
                    if (SessionStatus.InProgress == _sessionViewModel.Status)
                    {
                        var result = await MessageHelper.Instance.ShowMessageAsync("Все несохраненые изменения будут потеряны. Продолжить?", style: MessageDialogStyle.AffirmativeAndNegative);
                        if (MessageDialogResult.Negative == result) { return; }
                    }
                    _sessionViewModel.Clear();
                    MainTCSelectedIndex = (int)ViewIndex.SessionsView;
                    break;
                case ViewIndex.SessionDataView:
                    MainTCSelectedIndex = (int)ViewIndex.SessionsView;
                    break;
                case ViewIndex.TreatmentDataView:
                    MainTCSelectedIndex = (int)ViewIndex.TreatmentsView;
                    break;
                case ViewIndex.PatientView:
                    if (!_patientViewModel.IsSaved)
                    {
                        var result = await MessageHelper.Instance.ShowMessageAsync("Все несохраненые изменения будут потеряны. Продолжить?", style: MessageDialogStyle.AffirmativeAndNegative);
                        if (MessageDialogResult.Negative == result) { return;}
                    }
                    _patientViewModel.Clear();
                    MainTCSelectedIndex = (int) ViewIndex.PatientsView;
                    break;
                default:
                    MainTCSelectedIndex = (int) ViewIndex.PatientsView;
                    break;
            }
        }

        public async void OpetPatientTreatmentsHanlder(object sender, EventArgs eventArgs)
        {
            var cardioEventArgs = eventArgs as CardioEventArgs;
            if (null == cardioEventArgs) { return;}
            //var patientId = cardioEventArgs.Id;
            var patient = PatientsViewModel.SelectedPatient;
            TreatmentsViewModel.PatientName = new PatinetFullName
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
            var patientEventArgs = args as PatientEventArgs;
            if (null == patientEventArgs)
            {
                return;
            }
            _patientViewModel.Patient = patientEventArgs.Patient;
            _patientViewModel.AccessMode = patientEventArgs.Mode;
            MainTCSelectedIndex = (int) ViewIndex.PatientView;
        }

        public void StartOrContinueTreatmentSession(object sender, EventArgs args)
        {
            var treatmentArgs = args as TreatmentEventArgs;
            if (null != treatmentArgs)
            {
                switch (treatmentArgs.Action)
                {
                    case TreatmentAction.StartNew:

                        //here we crete new session
                        break;
                    case TreatmentAction.Continue:

                        //here we get existance session
                        break;
                    default:
                        return;
                }
                //pass params to SessionsView

                SessionsViewModel.PatientName = TreatmentsViewModel.PatientName;
                MainTCSelectedIndex = (int) ViewIndex.SessionsView;
            }
        }

        public void ShowTreatmentResults(object sender, EventArgs args)
        {
            var treatmentId = _treatmentsViewModel.SelectedTreatment.Id;
            //getting result
            MainTCSelectedIndex = (int) ViewIndex.TreatmentDataView;
        }

        private void StartSession(object sender, EventArgs args)
        {
            SessionViewModel.Patient = PatientsViewModel.SelectedPatient;
            MainTCSelectedIndex = (int) ViewIndex.SessionView;
        }

        private void ShowSessionResults(object sender, EventArgs args)
        {
            MainTCSelectedIndex = (int) ViewIndex.SessionDataView;
        }
    }
}
