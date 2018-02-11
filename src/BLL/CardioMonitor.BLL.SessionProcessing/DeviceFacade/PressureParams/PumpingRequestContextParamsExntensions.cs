using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal static class PumpingRequestContextParamsExntensions
    {
        [CanBeNull]
        public static PumpingRequestContextParams TryGetAutoPumpingRequestParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(PumpingRequestContextParams.AutoPumpingParamsId) as PumpingRequestContextParams;
        }
    }
}