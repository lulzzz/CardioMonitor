using System;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing
{
    internal class ExceptionContextParams : IContextParams
    {
        public static readonly Guid ExceptionContextParamsContextParamsId = new Guid("16aa0bb0-aa1b-47a1-a11c-af362f6bfa7b");

        public ExceptionContextParams(SessionProcessingException exception)
        {
            Exception = exception;
        }

        public Guid ParamsTypeId { get; }
        
        public SessionProcessingException Exception { get; }
    }

    internal static class ExceptionContextParamsPipelineConextExtensions
    {
        public static ExceptionContextParams TryGetExceptionContextParams([NotNull] this PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(ExceptionContextParams.ExceptionContextParamsContextParamsId) as
                ExceptionContextParams;
        }
    }
}