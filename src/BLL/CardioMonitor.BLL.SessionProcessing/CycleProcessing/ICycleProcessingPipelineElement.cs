using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing
{
    internal interface ICycleProcessingPipelineElement
    {
        Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context);

        bool CanProcess(CycleProcessingContext context);
    }
}