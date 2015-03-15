using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Patients;

namespace CardioMonitor.Patients.Session
{
    public class SessionDataViewModel : Notifier, IViewModel
    {
        private PatientFullName _patientName;
        private SessionModel _session;
        private ICommand _saveCommand;

        public PatientFullName PatientName
        {
            get { return _patientName; }
            set
            {
                if (value != _patientName)
                {
                    _patientName = value;
                    RisePropertyChanged("PatientName");
                }
            }
        }

        public SessionModel Session
        {
            get { return _session; }
            set
            {
                if (value != _session)
                {
                    _session = value;
                    RisePropertyChanged("Session");
                }
            }
        }

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


        public SessionDataViewModel()
        {
           
        }

        public async void SaveToFile()
        {
            await MessageHelper.Instance.ShowMessageAsync("Saved!");
        }

        public void Clear()
        {
            PatientName = null;
            Session = null;
        }
    }
}
