using System.Diagnostics.SymbolStore;
using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.ActionBlocks
{
    public interface IPipelineElement
    {
        Task<PipelineContext> ProcessAsync(PipelineContext context);

        bool CanProcess(PipelineContext context);
    }
}