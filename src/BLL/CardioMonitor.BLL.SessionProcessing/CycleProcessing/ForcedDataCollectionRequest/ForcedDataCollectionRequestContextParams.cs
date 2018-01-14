using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.ForcedDataCollectionRequest
{
    internal class ForcedDataCollectionRequestContextParams : IContextParams
    {
        public static readonly Guid ForcedDataCollectionRequestId = new Guid("e580965d-3f1f-4f68-a35f-ad2447394327");
        
        public Guid ParamsTypeId { get; }

        public ForcedDataCollectionRequestContextParams(bool isRequested)
        {
            IsRequested = isRequested;
        }

        public bool IsRequested { get; }
    }

    internal static class ForcedDataCollectionRequestContextParamsExntensions
    {
        public static ForcedDataCollectionRequestContextParams TryGetForcedDataCollectionRequest(
            [NotNull] this PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(ForcedDataCollectionRequestContextParams.ForcedDataCollectionRequestId) as ForcedDataCollectionRequestContextParams;
        }
    }
}