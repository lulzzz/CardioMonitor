using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal static class PumpingContextParamsExntensions
    {
        public static PumpingContextParams TryGetAutoPumpingParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(PumpingContextParams.AutoPumpingParamsId) as PumpingContextParams;
        }
    }
}