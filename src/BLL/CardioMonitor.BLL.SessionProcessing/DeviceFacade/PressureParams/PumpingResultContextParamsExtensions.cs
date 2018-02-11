using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal static class PumpingResultContextParamsExtensions
    {
        [CanBeNull]
        public static PumpingResultContextParams TryGetAutoPumpingResultParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(PumpingResultContextParams.PumpingResultParamsId) as PumpingResultContextParams;
        }
    }
}