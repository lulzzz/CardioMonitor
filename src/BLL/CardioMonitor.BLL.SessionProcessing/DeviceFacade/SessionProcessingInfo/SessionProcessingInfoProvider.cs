using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;
using Polly;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.SessionProcessingInfo
{
    /// <summary>
    /// Поставщик общей информации о сеансе
    /// </summary>
    internal class SessionProcessingInfoProvider : ICycleProcessingPipelineElement
    {
        [NotNull]
        private readonly IBedController _bedController;

        private readonly TimeSpan _bedControllerTimeout;
        
        
        public SessionProcessingInfoProvider([NotNull] IBedController bedController, TimeSpan bedControllerTimeout)
        {
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));
            _bedControllerTimeout = bedControllerTimeout;
        }

        public async Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context)
        {
            try
            {
                var timeoutPolicy = Policy.TimeoutAsync(_bedControllerTimeout);
                var elapsedTime = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetElapsedTimeAsync)
                    .ConfigureAwait(false);

                var remainingTime = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController .GetRemainingTimeAsync)
                    .ConfigureAwait(false);

                var cycleDuration = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCycleDurationAsync)
                    .ConfigureAwait(false);

                var cyclesCount = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCyclesCountAsync)
                    .ConfigureAwait(false);

                var currentCycleNumber = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCurrentCycleNumberAsync)
                    .ConfigureAwait(false);

                var sessionProcessingInfo = new SessionProcessingInfoContextParamses(
                    elapsedTime,
                    remainingTime,
                    cycleDuration,
                    currentCycleNumber,
                    cyclesCount);

                context.AddOrUpdate(sessionProcessingInfo);
            }
            catch (DeviceConnectionException ex)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.InversionTableConnectionError,
                            ex.Message,
                            ex)));
            }
            catch (DeviceProcessingException ex)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.InversionTableProcessingError,
                            ex.Message,
                            ex)));
               
            }
            catch (Exception ex)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.InversionTableProcessingError,
                            ex.Message,
                            ex)));
               
            }

            return context;
        }

        public bool CanProcess(CycleProcessingContext context)
        {
            return true;
        }
    }
}