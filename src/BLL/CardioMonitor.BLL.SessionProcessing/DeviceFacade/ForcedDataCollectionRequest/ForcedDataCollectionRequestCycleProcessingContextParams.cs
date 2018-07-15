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

        public Task ResultingTask { get; }

        public TaskCompletionSource<bool> PressureParamsSemaphore { get; }
        
        public TaskCompletionSource<bool> CommonParamsSemaphore { get; }

        public ForcedDataCollectionRequestCycleProcessingContextParams()
        {
            UniqObjectId = Guid.NewGuid();
            // чтобы ручной сбор данных завершался только после всех измерений
            // todo Добавить поддержку ЭКГ

            PressureParamsSemaphore = new TaskCompletionSource<bool>();
            CommonParamsSemaphore = new TaskCompletionSource<bool>();

            var waitingTasks = new[]
            {
                PressureParamsSemaphore.Task,
                CommonParamsSemaphore.Task
            };
            ResultingTask = Task.WhenAll(waitingTasks);
        }
       
        public void Dispose()
        {
            ResultingTask?.Dispose();
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