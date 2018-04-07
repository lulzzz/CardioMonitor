using System.Threading.Tasks;
using Markeli.Utils.Logging;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal interface ICycleProcessingPipelineElement
    {
        Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context);

        bool CanProcess(CycleProcessingContext context);

        void SetLogger(ILogger logger);
    }
}