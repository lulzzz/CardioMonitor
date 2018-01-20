using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.SessionProcessingInfo
{
    /// <summary>
    /// Поставщик общей информации о сеансе
    /// </summary>
    internal class SessionProcessingInfoProvider : ICycleProcessingPipelineElement
    {
        [NotNull]
        private readonly IBedController _bedController;

        public SessionProcessingInfoProvider(IBedController bedController)
        {
            _bedController = bedController;
        }

        public async Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context)
        { 
            try
            {
                var elapsedTime = await _bedController
                    .GetElapsedTimeAsync()
                    .ConfigureAwait(false);

                var remainingTime = await _bedController
                    .GetRemainingTimeAsync()
                    .ConfigureAwait(false);

                var cycleDuration = await _bedController
                    .GetCycleDurationAsync()
                    .ConfigureAwait(false);

                var cyclesCount = await _bedController
                    .GetCyclesCountAsync()
                    .ConfigureAwait(false);

                var currentCycleNumber = await _bedController
                    .GetCurrentCycleNumberAsync()
                    .ConfigureAwait(false);
                
                var sessionProcessingInfo = new SessionProcessingInfoContextParamses(
                    elapsedTime,
                    remainingTime,
                    cycleDuration,
                    currentCycleNumber,
                    cyclesCount);
                
                context.AddOrUpdate(sessionProcessingInfo);
            }
            catch (Exception e)
            {
              
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.InversionTableConnectionError,
                            e.Message,
                            e)));
            }

            return context;
        }

        public bool CanProcess(CycleProcessingContext context)
        {
            return true;
        }
    }
}