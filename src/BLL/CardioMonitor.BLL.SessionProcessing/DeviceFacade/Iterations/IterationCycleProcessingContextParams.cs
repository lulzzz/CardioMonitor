using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations
{
    internal class IterationCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid IterationCycleProcessingParamsId = new Guid("c5f4b1d5-e564-4ead-8189-2d841e1319d5");

        public Guid ParamsTypeId { get; } = IterationCycleProcessingParamsId;
        public Guid UniqObjectId { get; }

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
            UniqObjectId = Guid.NewGuid();
        }

        /// <summary>
        /// Номер текущей итерации
        /// </summary>
        public short CurrentIteration { get; }
        
        /// <summary>
        /// Номер итерации, в которой нужно запросить с кардиомонитора общие параметры
        /// </summary>
        public short IterationToGetCommonParams { get; }
        
        /// <summary>
        /// Номер итерации, в которой нужно запросить с кардиомонитора параметры давления
        /// </summary>
        public short IterationToGetPressureParams { get; }
        
        /// <summary>
        /// Номер итерации, в которой нужно запросить с кардиомонитора ЭКГ
        /// </summary>
        public short IterationToGetEcg { get; }
    }

    [CanBeNull]
    internal static class IterationCycleProcessingContextParamsExtensions
    {
        public static IterationCycleProcessingContextParams TryGetIterationParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(IterationCycleProcessingContextParams.IterationCycleProcessingParamsId) as IterationCycleProcessingContextParams;
        }
    }
}