using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal static class PressureParamsContextExntensions
    {
        public static PressureCycleProcessingContextParams TryGetPressureParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(PressureCycleProcessingContextParams.PressureParamsId) as PressureCycleProcessingContextParams;
        }
    }
}