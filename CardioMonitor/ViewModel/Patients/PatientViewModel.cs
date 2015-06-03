using System;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Repository.DataBase;
using CardioMonitor.Logs;
using CardioMonitor.Resources;
using CardioMonitor.ViewModel.Base;
using CardioMonitor.ViewModel.Communication;

namespace CardioMonitor.ViewModel.Patients
{
    public class PatientViewModel : Notifier, IViewModel
    {
        private AccessMode _accessMode;
        private string _lastName;
        private string _firstName;
        private string _patronymicName;
        private int _id;
        private DateTime? _birthDate;
        private ICommand _saveCommand;

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

        public PatientViewModel()
        {
            //_birthDate = DateTime.Now;
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? ( _saveCommand = new SimpleCommand
            {
                CanExecuteDelegate = x => !IsSaved,
                ExecuteDelegate = x => Save()
            }); }
        }

        private async void Save()
        {
            var message = String.Empty;
            try
            {
                switch (AccessMode)
                {
                    case AccessMode.Create:
                        DataBaseRepository.Instance.AddPatient(Patient);
                        message = Localisation.PatientViewModel_Patient_Added;
                        break;
                    case AccessMode.Edit:
                        DataBaseRepository.Instance.UpdatePatient(Patient);
                        message = Localisation.PatientViewModel_Patient_Updated;
                        break;
                }
                IsSaved = true;
                var hanlder = MoveBackwardEvent;
                if (hanlder != null)
                {
                    hanlder(null, null);
                }
            }
            catch (ArgumentNullException ex)
            {
                Logger.Instance.LogError("PatientViewModel",ex);
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

        public void Clear()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            PatronymicName = String.Empty;
            BirthDate = null;
        }
    }
}
