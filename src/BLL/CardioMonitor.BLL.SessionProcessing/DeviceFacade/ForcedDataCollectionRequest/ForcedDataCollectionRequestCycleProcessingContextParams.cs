using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest
{
    internal class ForcedDataCollectionRequestCycleProcessingContextParams : 
        ICycleProcessingContextParams,
        IDisposable
    {
        public static readonly Guid ForcedDataCollectionRequestId = new Guid("e580965d-3f1f-4f68-a35f-ad2447394327");

        public Guid ParamsTypeId { get; } = ForcedDataCollectionRequestId;
        public Guid UniqObjectId { get; }
        
        public SemaphoreSlim BlockingSemaphore { get; }

        public TaskCompletionSource<bool> PressureParamsSemaphore { get; }
        
        public TaskCompletionSource<bool> CommonParamsSemaphore { get; }
        
        public ForcedDataCollectionRequestCycleProcessingContextParams(
            bool isRequested)
        {
            IsRequested = isRequested;
            UniqObjectId = Guid.NewGuid();
            // чтобы ручной сбор данных завершался только после всех измерений
            // todo Добавить поддержку ЭКГ
            BlockingSemaphore = new SemaphoreSlim(0,1);
            
            PressureParamsSemaphore = new TaskCompletionSource<bool>();
            CommonParamsSemaphore = new TaskCompletionSource<bool>();

            Task.Factory.StartNew(async () =>
            {
                var waitingTasks = new[]
                {
                    PressureParamsSemaphore.Task,
                    CommonParamsSemaphore.Task
                };
                await Task
                    .WhenAll(waitingTasks)
                    .ConfigureAwait(false);
                BlockingSemaphore.Release();
            });
        }

        public bool IsRequested { get; }

        public void Dispose()
        {
            BlockingSemaphore?.Dispose();
        }
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