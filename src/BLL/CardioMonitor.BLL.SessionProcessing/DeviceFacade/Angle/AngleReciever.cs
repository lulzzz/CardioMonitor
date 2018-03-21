﻿using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;
using Polly;
using Polly.Timeout;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle
{
    internal class AngleReciever : ICycleProcessingPipelineElement
    {
        private readonly IBedController _bedController;
        private readonly TimeSpan _bedControllerTimeout;
        
        public AngleReciever(
            [NotNull] IBedController bedController, 
            TimeSpan bedControllerTimeout)
        {
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));
            _bedControllerTimeout = bedControllerTimeout;
        }
        
        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            var iterationInfo = context.TryGetIterationParams();
            var iterationNumber = iterationInfo?.CurrentIteration;
            
            try
            {
                var timeoutPolicy = Policy.TimeoutAsync(_bedControllerTimeout);

                var currentAngle = await timeoutPolicy.ExecuteAsync(_bedController
                        .GetAngleXAsync)
                    .ConfigureAwait(false);

                context.AddOrUpdate(new AngleXContextParams(currentAngle));
            }
            catch (DeviceConnectionException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.InversionTableConnectionError,
                            e.Message,
                            e,
                            cycleNumber,
                            iterationNumber)));
            }
            catch (TimeoutRejectedException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.UpdateAngleError, 
                            e.Message, 
                            e,
                            cycleNumber,
                            iterationNumber)));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.UpdateAngleError, 
                            e.Message, 
                            e,
                            cycleNumber,
                            iterationNumber)));
            }
           
            return context;
        }

        public bool CanProcess([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
    }
}