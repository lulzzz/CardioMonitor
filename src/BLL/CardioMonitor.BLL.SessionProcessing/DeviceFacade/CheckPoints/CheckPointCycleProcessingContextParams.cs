﻿using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints
{
    internal class CheckPointCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid CheckPointContextParamsId = new Guid("19338674-ba21-45cd-8bcc-1e6a9dddac24");

        public Guid ParamsTypeId { get; } = CheckPointContextParamsId;
        
        public Guid UniqObjectId { get; }

        public CheckPointCycleProcessingContextParams(
            bool needRequestEcg, 
            bool needRequestCommonParams, 
            bool needRequestPressureParams)
        {
            NeedRequestEcg = needRequestEcg;
            NeedRequestCommonParams = needRequestCommonParams;
            NeedRequestPressureParams = needRequestPressureParams;
            UniqObjectId = Guid.NewGuid();
        }

        public bool NeedRequestEcg { get; }
        
        public bool NeedRequestCommonParams{ get; }
        
        public bool NeedRequestPressureParams { get; }
    }

    [CanBeNull]
    internal static class CheckPointParamsContextExnteions
    {
        public static CheckPointCycleProcessingContextParams TryGetCheckPointParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(CheckPointCycleProcessingContextParams.CheckPointContextParamsId) as CheckPointCycleProcessingContextParams;
        }
    }
}