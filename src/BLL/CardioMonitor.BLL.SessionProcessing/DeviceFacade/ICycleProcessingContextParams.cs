using System;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal interface ICycleProcessingContextParams
    {
        /// <summary>
        /// Уникальный идентификатор типа параметра
        /// </summary>
        Guid ParamsTypeId { get; }
        
        /// <summary>
        /// Уникальный идентификатор объекта
        /// </summary>
        Guid UniqObjectId { get; }
    }
}