﻿using System;
using System.Threading;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest
{
    internal class ForcedDataCollectionRequestCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid ForcedDataCollectionRequestId = new Guid("e580965d-3f1f-4f68-a35f-ad2447394327");

        public Guid ParamsTypeId { get; } = ForcedDataCollectionRequestId;
        public Guid UniqObjectId { get; }
        
        public SemaphoreSlim BlockingSemaphore { get; }

        public ForcedDataCollectionRequestCycleProcessingContextParams(
            bool isRequested)
        {
            IsRequested = isRequested;
            UniqObjectId = Guid.NewGuid();
            // чтобы ручной сбор данных завершался только после всех измерений
            // todo Добавить поддержку ЭКГ
            BlockingSemaphore = new SemaphoreSlim(DeviceFacadeConstants.ForcedRequestBlockCounts);
            for (var i = 0; i < DeviceFacadeConstants.ForcedRequestBlockCounts; ++i)
            {
                BlockingSemaphore.Wait();
            }
        }

        public bool IsRequested { get; }
    }

    [CanBeNull]
    internal static class ForcedDataCollectionRequestContextParamsExntensions
    {
        public static ForcedDataCollectionRequestCycleProcessingContextParams TryGetForcedDataCollectionRequest(
            [NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(ForcedDataCollectionRequestCycleProcessingContextParams.ForcedDataCollectionRequestId) as ForcedDataCollectionRequestCycleProcessingContextParams;
        }
    }
}