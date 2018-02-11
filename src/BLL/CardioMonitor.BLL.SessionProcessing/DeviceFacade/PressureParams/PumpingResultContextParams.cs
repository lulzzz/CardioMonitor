using System;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    /// <summary>
    /// Результаты накачки
    /// </summary>
    public class PumpingResultContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid PumpingResultParamsId = new Guid("847b407c-1336-45fd-8f8f-8808e8594250");

        public Guid ParamsTypeId { get; } = PumpingResultParamsId;
        
        public Guid UniqObjectId { get; }
        
        public bool WasPumpingCompleted { get; }
        
        public PumpingResultContextParams(bool wasPumpingCompleted)
        {
            WasPumpingCompleted = wasPumpingCompleted;
            UniqObjectId = Guid.NewGuid();
        }
    }
}