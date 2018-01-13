using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    internal interface IPipelineElement
    {
        Task<PipelineContext> ProcessAsync(PipelineContext context);

        bool CanProcess(PipelineContext context);
    }
}