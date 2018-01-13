using System;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    public interface IContextParams
    {
        Guid ParamsTypeId { get; }
    }
}