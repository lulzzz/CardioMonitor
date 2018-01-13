using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    internal interface IPipeline
    {
        Task StartAsync();

        Task EmergencyStopAsync();

        Task PauseAsync();

        Task ResetAsync();

        void ProcessReverseRequest();
    }
}