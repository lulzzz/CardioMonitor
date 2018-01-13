using System;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    internal interface IContextParams
    {
        Guid ParamsTypeId { get; }
    }
}