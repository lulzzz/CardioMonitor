using System;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations
{
    internal class IterationCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid IterationCycleProcessingParamsId = new Guid("c5f4b1d5-e564-4ead-8189-2d841e1319d5");

        public Guid ParamsTypeId { get; } = IterationCycleProcessingParamsId;

        public IterationCycleProcessingContextParams(
            short currentIteration, 
            short iterationToGetCommonParams, 
            short iterationToGetPressureParams, 
            short iterationToGetEcg)
        {
            CurrentIteration = currentIteration;
            IterationToGetCommonParams = iterationToGetCommonParams;
            IterationToGetPressureParams = iterationToGetPressureParams;
            IterationToGetEcg = iterationToGetEcg;
        }

        public short CurrentIteration { get; }
        
        public short IterationToGetCommonParams { get; }
        
        public short IterationToGetPressureParams { get; }
        
        public short IterationToGetEcg { get; }
    }

    internal static class IterationCycleProcessingContextParamsExtensions
    {
        public static IterationCycleProcessingContextParams TryGetIterationParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(IterationCycleProcessingContextParams.IterationCycleProcessingParamsId) as IterationCycleProcessingContextParams;
        }
    }
}