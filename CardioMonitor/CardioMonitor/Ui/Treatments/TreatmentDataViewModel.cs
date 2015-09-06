﻿using System;
using System.Windows.Input;
using CardioMonitor.Infrastructure.Models.Patients;
using CardioMonitor.Infrastructure.Models.Treatment;
using CardioMonitor.Infrastructure.Ui;
using CardioMonitor.Infrastructure.Ui.Base;
using CardioMonitor.ViewModel;

namespace CardioMonitor.Ui.Treatments
{
    public class TreatmentDataViewModel : Notifier
    {
        private TreatmentFullStatistic _statistic;
        private PatientFullName _patientName;
        private DateTime _startDate;

        private ICommand _saveAllCommand;
        private ICommand _saveSingleCommand;

        public TreatmentFullStatistic Statistic
        {
            get { return _statistic; }
            set
            {
                if (value != _statistic)
                {
                    _statistic = value;
                    RisePropertyChanged("Statistic");
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

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (value != _startDate)
                {
                    _startDate = value;
                    RisePropertyChanged("StartDate");
                }
            }
        }

        public ICommand SaveAllCommand
        {
            get
            {
                return _saveAllCommand ?? (_saveAllCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x => SaveAll()
                });
            }
        }

        public ICommand SaveSingleCommand
        {
            get
            {
                return _saveSingleCommand ?? (_saveSingleCommand = new SimpleCommand
                {
                    ExecuteDelegate = x => SaveSingle(x)
                });
            }
        }

        private void SaveAll()
        {
            MessageHelper.Instance.ShowMessageAsync("Save all!");
        }

        private void SaveSingle(object statistic)
        {
            MessageHelper.Instance.ShowMessageAsync("Save single!");
        }
    }
}
