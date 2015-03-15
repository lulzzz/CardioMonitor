using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Core.Repository;

namespace CardioMonitor.Patients
{
    public class PatientViewModel : Notifier, IViewModel
    {
        private AccessMode _accessMode;
        private string _lastName;
        private string _firstName;
        private string _patronymicName;
        private int _id;
        private string _title;
        private ICommand _saveCommand;

        public EventHandler MoveFowardEvent { get; set; }

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
                    Id = _id
                };
            }

            set
            {
                _id = value.Id;
                LastName = value.LastName;
                FirstName = value.FirstName;
                PatronymicName = value.PatronymicName;
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

        public string Title
        {
            get 
            {
                return (AccessMode.Create == AccessMode)? "Добавление пациента":"Редактирование данных пациента";
            }
            set
            {
                RisePropertyChanged("Title");
            }
        }

        public bool IsSaved { get; set; }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? ( _saveCommand = new SimpleCommand
            {
                CanExecuteDelegate = x => !IsSaved,
                ExecuteDelegate = x => Save()
            }); }
        }

        private void Save()
        {
            try
            {
                switch (AccessMode)
                {
                    case AccessMode.Create:
                        Repository.Instance.AddPatient(Patient);
                        MessageHelper.Instance.ShowMessageAsync("Новый пациент добавлен!");
                        break;
                    case AccessMode.Edit:
                        Repository.Instance.UpdatePatient(Patient);
                        MessageHelper.Instance.ShowMessageAsync("Изменения сохранены!");
                        break;
                }
                IsSaved = true;
            }
            catch (Exception ex)
            {
                switch (AccessMode)
                {
                    case AccessMode.Create:
                        MessageHelper.Instance.ShowMessageAsync("Не удалось добавить нового пациента!");
                        break;
                    case AccessMode.Edit:
                        MessageHelper.Instance.ShowMessageAsync("Не удалось сохранить изменения!");
                        break;
                }
            }
        }

        public void Clear()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            PatronymicName = String.Empty;
        }
    }
}
