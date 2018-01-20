using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal static class AutoPumpingContextParamsExntensions
    {
        public static AutoPumpingContextParams TryGetAutoPumpingParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(AutoPumpingContextParams.AutoPumpingParamsId) as AutoPumpingContextParams;
        }
    }
}