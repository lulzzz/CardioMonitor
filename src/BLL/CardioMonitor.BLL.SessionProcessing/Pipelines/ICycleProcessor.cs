using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    internal interface ICycleProcessor
    {
        Task StartAsync();

        Task StopAsync();

        Task PauseAsync();

        Task ResetAsync();

        void ProcessReverseRequest();

        Task ForceDataCollectionRequestAsync();
    }
}