using System;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal interface ICycleProcessingContextParams
    {
        Guid ParamsTypeId { get; }
    }
}