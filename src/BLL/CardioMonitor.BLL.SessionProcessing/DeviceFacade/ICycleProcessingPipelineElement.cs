using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal interface ICycleProcessingPipelineElement
    {
        Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context);

        bool CanProcess(CycleProcessingContext context);
    }
}