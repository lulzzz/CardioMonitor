using System;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions
{
    internal class ExceptionCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid ExceptionContextParamsContextParamsId = new Guid("16aa0bb0-aa1b-47a1-a11c-af362f6bfa7b");

        public ExceptionCycleProcessingContextParams(SessionProcessingException exception)
        {
            Exception = exception;
        }

        public Guid ParamsTypeId { get; } = ExceptionContextParamsContextParamsId;
        
        public SessionProcessingException Exception { get; }
    }

    internal static class ExceptionContextParamsPipelineConextExtensions
    {
        public static ExceptionCycleProcessingContextParams TryGetExceptionContextParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(ExceptionCycleProcessingContextParams.ExceptionContextParamsContextParamsId) as
                ExceptionCycleProcessingContextParams;
        }
    }
}