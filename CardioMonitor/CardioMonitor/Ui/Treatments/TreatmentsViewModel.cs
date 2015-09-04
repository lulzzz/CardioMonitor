using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CardioMonitor.Infrastructure.Models.Patients;
using CardioMonitor.Infrastructure.Models.Treatment;
using CardioMonitor.Infrastructure.Ui.Base;
using CardioMonitor.ViewModel;
using CardioMonitor.ViewModel.Communication;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.Ui.Treatments
{
    public class TreatmentsViewModel : Notifier, IViewModel
    {
        private ObservableCollection<Treatment> _treatments;
        private PatientFullName _patientName;
        private Treatment _selectedTreatment;

        private ICommand _startNewCommand;
        private ICommand _continueCommand;
        private ICommand _deleteCommand;
        private ICommand _showResultsCommand;

        public ObservableCollection<Treatment> Treatments
        {
            get { return _treatments; }
            set
            {
                if (value != _treatments)
                {
                    _treatments = value;
                    RisePropertyChanged("Treatments");
                }
            }
        }

        public PatientFullName PatientName
        {
            get { return _patientName; }
            set
            {
                if (value != _patientName)
                {
                    _patientName = value;
                    RisePropertyChanged("Patient");
                }
            }
        }

        public Treatment SelectedTreatment
        {
            get { return _selectedTreatment; }
            set
            {
                if (value != _selectedTreatment)
                {
                    _selectedTreatment = value;
                    RisePropertyChanged("SelectedTreatment");
                }
            }
        }

        public ICommand StartNewCommand
        {
            get
            {
                return _startNewCommand ?? (_startNewCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x => StartNew()
                });
            }
        }

        public ICommand ContinueCommand
        {
            get
            {
                return _continueCommand ?? (_continueCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x=> null != SelectedTreatment,
                    ExecuteDelegate = x=> Continue(x)
                });
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x=> null != SelectedTreatment,
                    ExecuteDelegate = x => Delete(x)
                });
            }
        }

        public ICommand ShowResultsCommand
        {
            get
            {
                return _showResultsCommand ?? (_showResultsCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x =>  null != SelectedTreatment,
                    ExecuteDelegate = x=> ShowResults(x)
                });
            }
        }

        public EventHandler OpenSessionsEvent { get; set; }
        public EventHandler ShowResultsEvent { get; set; }

        public TreatmentsViewModel()
        {
            Treatments = new ObservableCollection<Treatment>();
        }

        private void StartNew()
        {
            var hanlder = OpenSessionsEvent;
            if (null != hanlder)
            {
                hanlder(this, new TreatmentEventArgs {Action = TreatmentAction.StartNew });
            }
        }

        private void Continue(object sender)
        {
            var hanlder = OpenSessionsEvent;
            if (null != hanlder)
            {
                hanlder(this, new TreatmentEventArgs { Action = TreatmentAction.Continue });
            }
        }

        private async void Delete(object sender)
        {
            var result = await MessageHelper.Instance.ShowMessageAsync("Вы уверены, что хотите удалить курс обследования?",
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative == result)
            {
                var treatment = sender as Treatment;
                if (null != treatment)
                {
                    Treatments.Remove(treatment);
                }
                else
                {
                    await MessageHelper.Instance.ShowMessageAsync("Не удалось удалить курс обследования");
                }
            }
        }

        private void ShowResults(object sender)
        {
            var hanlder = ShowResultsEvent;
            if (null != hanlder)
            {
                hanlder(this, null);
            }
        }

        public void Clear()
        {
            Treatments = new ObservableCollection<Treatment>();
            PatientName = null;
            SelectedTreatment = null;
        }
    }
}
