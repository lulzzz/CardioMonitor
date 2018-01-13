using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.CheckPoints
{
    internal class CheckPointContextParams : IContextParams
    {
        public static readonly Guid CheckPointContextParamsId = new Guid("19338674-ba21-45cd-8bcc-1e6a9dddac24");
        
        public Guid ParamsTypeId { get; }

        public CheckPointContextParams(bool isCheckPointReached, bool isMaxCheckPoint)
        {
            IsCheckPointReached = isCheckPointReached;
            IsMaxCheckPoint = isMaxCheckPoint;
        }

        public bool IsCheckPointReached { get; }
        
        public bool IsMaxCheckPoint { get; }
    }

    internal static class CheckPointParamsContextExnteions
    {
        public static CheckPointContextParams TryGetCheckPointParams([NotNull] this PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(CheckPointContextParams.CheckPointContextParamsId) as CheckPointContextParams;
        }
    }
}