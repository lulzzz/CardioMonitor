using System;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions
{
    internal interface ICycleProcessingContextParams
    {
        Guid ParamsTypeId { get; }
    }
}