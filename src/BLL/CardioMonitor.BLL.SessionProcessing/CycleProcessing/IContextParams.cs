using System;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing
{
    internal interface IContextParams
    {
        Guid ParamsTypeId { get; }
    }
}