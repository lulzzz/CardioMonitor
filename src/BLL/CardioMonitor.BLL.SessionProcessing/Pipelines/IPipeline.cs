using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    public interface IPipeline
    {
        Task StartAsync();

        Task EmergencyStopAsync();

        Task PauseAsync();

        Task ResetAsync();
    }
}