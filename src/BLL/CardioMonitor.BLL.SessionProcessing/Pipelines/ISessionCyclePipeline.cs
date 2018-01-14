using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    internal interface ISessionCyclePipeline
    {
        Task StartAsync();

        Task StopAsync();

        Task PauseAsync();

        Task ResetAsync();

        void ProcessReverseRequest();
    }
}