using System;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints
{
    internal class CheckPointCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid CheckPointContextParamsId = new Guid("19338674-ba21-45cd-8bcc-1e6a9dddac24");
        
        public Guid ParamsTypeId { get; }

        public CheckPointCycleProcessingContextParams(bool isCheckPointReached, bool isMaxCheckPoint)
        {
            IsCheckPointReached = isCheckPointReached;
            IsMaxCheckPoint = isMaxCheckPoint;
        }

        public bool IsCheckPointReached { get; }
        
        public bool IsMaxCheckPoint { get; }
    }

    internal static class CheckPointParamsContextExnteions
    {
        public static CheckPointCycleProcessingContextParams TryGetCheckPointParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(CheckPointCycleProcessingContextParams.CheckPointContextParamsId) as CheckPointCycleProcessingContextParams;
        }
    }
}