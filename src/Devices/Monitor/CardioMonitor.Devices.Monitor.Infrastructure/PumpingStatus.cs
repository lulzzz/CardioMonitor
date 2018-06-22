namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    public class PumpingStatus
    {
        public bool IsPumpingError { get; set; }

        public bool IsPumpingInProgress { get; set; }

        public PumpingStatus(bool isPumpingError, bool isPumpingInProgress)
        {
            IsPumpingError = isPumpingError;
            IsPumpingInProgress = isPumpingInProgress;
        }
    }
}
