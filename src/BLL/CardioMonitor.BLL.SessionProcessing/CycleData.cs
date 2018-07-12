using System.Collections.ObjectModel;

namespace CardioMonitor.BLL.SessionProcessing
{
    public class CycleData
    {
        public short CycleNumber { get; set; }

        public ObservableCollection<CheckPointParams> CycleParams { get; set; }
    }
}