﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Logs;
using CardioMonitor.Models.Patients;
using CardioMonitor.Models.Session;
using CardioMonitor.Models.Treatment;
using CardioMonitor.Repositories;
using CardioMonitor.Repositories.Abstract;
using CardioMonitor.Resources;
using CardioMonitor.Settings;
using CardioMonitor.Statistics;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.Communication;
using CardioMonitor.Ui.ViewModel.Patients;
using CardioMonitor.Ui.ViewModel.Sessions;
using CardioMonitor.Ui.ViewModel.Settings;
using CardioMonitor.Ui.ViewModel.Treatments;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.Ui.ViewModel{

    internal enum ViewIndex
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
        private readonly ILogger _logger;
        private readonly IPatientsRepository _patientsRepository;
        private readonly ITreatmentsRepository _treatmentsRepository;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IFilesManager _filesRepository;
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

        #region Свойства

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

#endregion

        public MainWindowViewModel(
            ILogger logger,
            IPatientsRepository patientsRepository,
            ITreatmentsRepository treatmentsRepository,
            ISessionsRepository sessionsRepository,
            IFilesManager filesRepository,
            IDataBaseRepository dataBaseRepository,
            IDeviceControllerFactory deviceControllerFactory,
            ICardioSettings settings,
            TaskHelper taskHelper)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (patientsRepository == null) throw new ArgumentNullException(nameof(patientsRepository));
            if (treatmentsRepository == null) throw new ArgumentNullException(nameof(treatmentsRepository));
            if (sessionsRepository == null) throw new ArgumentNullException(nameof(sessionsRepository));
            if (filesRepository == null) throw new ArgumentNullException(nameof(filesRepository));
            if (deviceControllerFactory == null) throw new ArgumentNullException(nameof(deviceControllerFactory));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));

            _logger = logger;
            _patientsRepository = patientsRepository;
            _treatmentsRepository = treatmentsRepository;
            _sessionsRepository = sessionsRepository;
            _filesRepository = filesRepository;

            PatientsViewModel = new PatientsViewModel(patientsRepository)
            {
                OpenPatienEvent = OpetPatientTreatmentsHanlder,
                AddEditPatient = AddEditPatientHanlder,
                OpenSessionsHandler = StartOrContinueTreatmentSession,
                ShowTreatmentResults = ShowTreatmentResults,
                OpenSessionHandler = LoadSession
            };
            PatientViewModel = new PatientViewModel(logger, patientsRepository)
            {
                MoveBackwardEvent = MoveBackwardPatient
            };
            TreatmentsViewModel = new TreatmentsViewModel
            {
                OpenSessionsEvent = StartOrContinueTreatmentSession,
                ShowResultsEvent = ShowTreatmentResults
            };
            SessionsViewModel = new SessionsViewModel(sessionsRepository)
            {
                StartSessionEvent = StartSession,
                ShowResultsEvent = ShowSessionResults
            };
            SessionViewModel = new SessionViewModel(logger, filesRepository, sessionsRepository, deviceControllerFactory, taskHelper);
            //SessionViewModel.StartStatusTimer();
            SessionDataViewModel = new SessionDataViewModel(logger, filesRepository);
            
            TreatmentDataViewModel = new TreatmentDataViewModel();
            SettingsViewModel = new SettingsViewModel(dataBaseRepository, settings);
        }

        public void UpdatePatiens()
        {
            var message = String.Empty;
            try
            {
                var patients = _patientsRepository.GetPatients();
                PatientsViewModel.Patients = patients != null
                    ? new ObservableCollection<Patient>(patients) 
                    : new ObservableCollection<Patient>();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!String.IsNullOrEmpty(message))
            {

                MessageHelper.Instance.ShowMessageAsync(message);
            }
        }

        private void MoveBackwardPatient(object sender, EventArgs args)
        {
            MoveBackwardView((int)ViewIndex.PatientView);
        }

        private async void MoveBackwardView(object sender)
        {
            int index;
            if (!int.TryParse(sender.ToString(), out index)) { return; }
            var viewIndex = (ViewIndex)index;
            switch (viewIndex)
            {
                case ViewIndex.TreatmentsView:
                    TreatmentsViewModel.Clear();
                    UpdatePatiens();
                    MainTCSelectedIndex = (int)ViewIndex.PatientsView;
                    break;
                case ViewIndex.SessionsView:
                    SessionsViewModel.Clear();
                    UpdatePatiens();
                    MainTCSelectedIndex = (int)ViewIndex.PatientsView;
                    //MainTCSelectedIndex = (int)ViewIndex.TreatmentsView;
                    break;
                case ViewIndex.SessionView:
                    if (SessionStatus.InProgress == SessionViewModel.Status)
                    {
                        var result = await MessageHelper.Instance.ShowMessageAsync(Localisation.LostChangesConfirmation, style: MessageDialogStyle.AffirmativeAndNegative);
                        if (MessageDialogResult.Negative == result) { return; }
                    }
                    SessionViewModel.Clear();
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
                    if (!PatientViewModel.IsSaved)
                    {
                        var result = await MessageHelper.Instance.ShowMessageAsync(Localisation.LostChangesConfirmation, 
                                                                                    style: MessageDialogStyle.AffirmativeAndNegative);
                        if (MessageDialogResult.Negative == result) { return;}
                    }
                    PatientViewModel.Clear();
                    UpdatePatiens();
                    MainTCSelectedIndex = (int) ViewIndex.PatientsView;
                    break;
                default:
                    UpdatePatiens();
                    MainTCSelectedIndex = (int) ViewIndex.PatientsView;
                    break;
            }
        }

        public void OpetPatientTreatmentsHanlder(object sender, EventArgs eventArgs)
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
                PatientViewModel.Patient = patientEventArgs.Patient;
                PatientViewModel.AccessMode = patientEventArgs.Mode;
                MainTCSelectedIndex = (int)ViewIndex.PatientView;
            }
            catch (Exception ex)
            {
                _logger.LogError("MainWindowViewModel",ex);
            }
        }

        public async void StartOrContinueTreatmentSession(object sender, EventArgs args)
        {
            //TODO Old realisation
            var message = String.Empty;
            var patient = PatientsViewModel.SelectedPatient;
            try
            {

                var treatments = _treatmentsRepository.GetTreatments(patient.Id);
                var treatment = treatments.FirstOrDefault();
                if (null == treatment)
                {
                    _treatmentsRepository.AddTreatment(new Treatment { StartDate = DateTime.Now, PatientId = patient.Id });
                    treatment = _treatmentsRepository.GetTreatments(patient.Id).FirstOrDefault();
                    if (null == treatment)
                    {
                        await MessageHelper.Instance.ShowMessageAsync(Localisation.MainWindowViewModel_CantLoadList);
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

        private void UpdateSessionInfos()
        {
            var sessions = _sessionsRepository.GetSessionInfos(SessionsViewModel.Treatment.Id);
            SessionsViewModel.SessionInfos = sessions != null
                ? new ObservableCollection<SessionInfo>(sessions)
                : new ObservableCollection<SessionInfo>();
        }

        private async void UpdateSessionInfosSave()
        {
            var message = String.Empty;
            try
            {
                UpdateSessionInfos();
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

        //temporary not  used
        public void ShowTreatmentResults(object sender, EventArgs args)
        {
            //var treatmentId = _treatmentsViewModel.SelectedTreatment.Id;
            //getting result
            var session = new SessionModel {DateTime = new DateTime()};
            
            var statisticBuilder = new TreatmentStatisticBuilder();
            TreatmentDataViewModel.Statistic = statisticBuilder.Build(new[]
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
            SessionViewModel.StartStatusTimer();
            MainTCSelectedIndex = (int) ViewIndex.SessionView;
        }

        private async void ShowSessionResults(object sender, EventArgs args)
        {
            var message = String.Empty;
            try
            {

                var session = _sessionsRepository.GetSession(SessionsViewModel.SelectedSessionInfo.Id);
                SessionDataViewModel.Session = new SessionModel {Session = session};
                SessionDataViewModel.Patient = PatientsViewModel.SelectedPatient;
                MainTCSelectedIndex = (int)ViewIndex.SessionDataView;
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

        private async void LoadSession(object sender, EventArgs args)
        {
            var loadDialog = new OpenFileDialog {CheckFileExists = true, Filter = Localisation.FileRepository_SeansFileFilter};
            var result = loadDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var message = String.Empty;
                try
                {
                    var container = _filesRepository.LoadFromFile(loadDialog.FileName);
                    SessionDataViewModel.Session = new SessionModel {Session = container.Session};
                    SessionDataViewModel.Patient = container.Patient;
                    MainTCSelectedIndex = (int) ViewIndex.SessionDataView;
                }
                catch (ArgumentNullException)
                {
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
        }
    }

  
}
