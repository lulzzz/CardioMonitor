using System.Windows.Input;
using CardioMonitor.Infrastructure.Ui.Base;
using CardioMonitor.Infrastructure.Ui.Sessions;

namespace CardioSessionViewer
{
    public class MainWindowViewModel : Notifier
    {
        private SessionDataViewModel _sessionDataViewModel;

        public SessionDataViewModel SessionDataViewModel
        {
            get { return _sessionDataViewModel; }
            set
            {
                _sessionDataViewModel = value;
                RisePropertyChanged("SessionDataViewModel");
            }
        }

        #region Команды

        private ICommand _openSessionComand;
        private ICommand _saveSessionComand;

        public ICommand OpenSessionCommand
        {
            get
            {
                return _openSessionComand ?? (_openSessionComand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x => OpenSessionExecute()
                });
            }
        }
        public ICommand SaveSessionCommand
        {
            get
            {
                return _saveSessionComand ?? (_saveSessionComand = new SimpleCommand
                {
                    CanExecuteDelegate = x => CanSaveSessionExecute(),
                    ExecuteDelegate = x => SaveSessionExecute()
                });
            }
        }

        #endregion

        public MainWindowViewModel()
        {
            SessionDataViewModel = new SessionDataViewModel();
        }

        public void OpenSessionExecute()
        {
            if (SessionDataViewModel == null) return;

            SessionDataViewModel.OpenFromFile();
        }

        public void SaveSessionExecute()
        {
            if (SessionDataViewModel == null) return;

            SessionDataViewModel.SaveToFile();
        }

        public bool CanSaveSessionExecute()
        {
            return true;
        }
    }
}
