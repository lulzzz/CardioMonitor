using System;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal class PumpingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid AutoPumpingParamsId = new Guid("82c3d092-71d1-7e6e-b3d2-21a4ca9d248c");
        
        public Guid ParamsTypeId { get; } = AutoPumpingParamsId;
        public Guid UniqObjectId { get; }

        public PumpingContextParams(
            bool isAutoPumpingEnabled, 
            short pumpingNumberOfAttempts)
        {
            IsAutoPumpingEnabled = isAutoPumpingEnabled;
            PumpingNumberOfAttempts = pumpingNumberOfAttempts;
            UniqObjectId = Guid.NewGuid();
        }

        /// <summary>
        /// Признак необходимости выполнения автонакачки манжеты
        /// </summary>
        public bool IsAutoPumpingEnabled { get; }
        
        public short PumpingNumberOfAttempts { get; }
    }
}