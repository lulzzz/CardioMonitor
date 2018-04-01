using System.Collections.ObjectModel;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Ui.Base;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionCycleViewModel : Notifier
    {
        private int _cycleNumber;

        private ObservableCollection<PatientParams> _patientParams;

        public int CycleNumber
        {
            get {return _cycleNumber; }
            set
            {
                _cycleNumber = value;
                RisePropertyChanged(nameof(CycleNumber));
            }
        }
        /// <summary>
        /// Показатели пациента
        /// </summary>
        public ObservableCollection<PatientParams> PatientParams
        {
            get { return _patientParams; }
            set
            {
                if (value != _patientParams)
                {
                    _patientParams = value;
                    RisePropertyChanged(nameof(PatientParams));
                }
            }
        }

        //todo ecg here
    }
}
