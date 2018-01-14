using System;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.Exceptions
{
    internal interface ICycleProcessingContextParams
    {
        Guid ParamsTypeId { get; }
    }
}