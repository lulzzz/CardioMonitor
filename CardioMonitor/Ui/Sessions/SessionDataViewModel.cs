﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Models.Session;
using CardioMonitor.Core.Repository.DataBase;
using CardioMonitor.Core.Repository.Files;
using CardioMonitor.Logs;
using CardioMonitor.Resources;
using CardioMonitor.ViewModel.Base;

namespace CardioMonitor.ViewModel.Sessions
{
    public class SessionDataViewModel : Notifier, IViewModel
    {
        private PatientFullName _patientName;
        private Patient _patient;
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

        public Patient Patient
        {
            get { return _patient; }
            set
            {
                if (value != _patient)
                {
                    _patient = value;

                    PatientName = _patient != null
                        ? new PatientFullName
                        {
                            LastName = _patient.LastName,
                            FirstName = _patient.FirstName,
                            PatronymicName = _patient.PatronymicName,
                        }
                        : new PatientFullName();
                    RisePropertyChanged("Patient");
                    RisePropertyChanged("Patients");
                }
            }
        }

        public ObservableCollection<Patient> Patients
        {
            get { return Patient != null 
                    ? new ObservableCollection<Patient> { Patient }
                    : new ObservableCollection<Patient>(); 
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

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RisePropertyChanged("IsReadOnly");
            }
        }
        private bool _isReadOnly;

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
            IsReadOnly = true;
        }

        public async void SaveToFile()
        {
            var saveFileDialog = new SaveFileDialog {Filter = Localisation.FileRepository_SeansFileFilter};
            var dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {

                var exceptionMessage = String.Empty;
                try
                {
                    FileRepository.SaveToFile(Patient, Session.Session, saveFileDialog.FileName);
                    await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionDataViewModel_FileSaved);
                }
                catch (ArgumentNullException ex)
                {
                    Logger.Instance.LogError("SessionViewModel", ex);
                    exceptionMessage = Localisation.SessionViewModel_SaveSession_ArgumentNullException;
                }
                catch (Exception ex)
                {
                    exceptionMessage = ex.Message;
                }
                if (!String.IsNullOrEmpty(exceptionMessage))
                {
                    await MessageHelper.Instance.ShowMessageAsync(exceptionMessage);
                }
            }
        }

        public async void LoadFromFile()
        {
            await MessageHelper.Instance.ShowMessageAsync("Opened!");
        }

        public void Clear()
        {
            PatientName = null;
            Session = null;
            Patient = null;
        }
    }
}
