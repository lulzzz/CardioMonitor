﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints
{
    internal class IterationBasedCheckPointChecker : ICycleProcessingPipelineElement
    {
        private readonly HashSet<IterationBasedCheckPoint> _commonParamsCheckPoints;
        private readonly HashSet<IterationBasedCheckPoint> _pressureParamsCheckPoints;
        private readonly HashSet<IterationBasedCheckPoint> _ecgParamsCheckPoints;
        
        public IterationBasedCheckPointChecker()
        {
            _commonParamsCheckPoints = new HashSet<IterationBasedCheckPoint>();
            _pressureParamsCheckPoints = new HashSet<IterationBasedCheckPoint>();
            _ecgParamsCheckPoints = new HashSet<IterationBasedCheckPoint>();
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            await Task.Yield();

            var sessionProcessingInfo = context.TryGetSessionProcessingInfo();
            if (sessionProcessingInfo == null) return context;
            
            var iterationParams = context.TryGetIterationParams();
            if (iterationParams == null) return context;

            try
            {
                var currentCycleNumber = sessionProcessingInfo.CurrentCycleNumber;
                var currentIteration = iterationParams.CurrentIteration;
                var nextIterationToEcgMeassuring = iterationParams.IterationToGetEcg;
                var nextIterationToCommonParamsMeassuring = iterationParams.IterationToGetCommonParams;
                var nextIterationToPressureParamsMeassuring = iterationParams.IterationToGetPressureParams;

                var ecgCheckPoint = new IterationBasedCheckPoint(currentCycleNumber, nextIterationToEcgMeassuring);
                var commonParamsCheckPoint = new IterationBasedCheckPoint(currentCycleNumber, nextIterationToCommonParamsMeassuring);
                var pressureParamsCheckPoint = new IterationBasedCheckPoint(currentCycleNumber, nextIterationToPressureParamsMeassuring);

                var needRequestEcg = false;
                if (currentIteration == nextIterationToEcgMeassuring)
                {
                    needRequestEcg = _ecgParamsCheckPoints.Contains(ecgCheckPoint);
                    if (needRequestEcg)
                    {
                        _ecgParamsCheckPoints.Add(ecgCheckPoint);
                    }
                }
                var needRequestCommonParams = false;
                if (currentIteration == nextIterationToCommonParamsMeassuring)
                {
                    needRequestCommonParams = _commonParamsCheckPoints.Contains(commonParamsCheckPoint);
                    if (needRequestCommonParams)
                    {
                        _commonParamsCheckPoints.Add(commonParamsCheckPoint);
                    }
                }
                var needRequestPressureParams = false;
                if (currentIteration == nextIterationToPressureParamsMeassuring)
                {
                    needRequestPressureParams = _pressureParamsCheckPoints.Contains(pressureParamsCheckPoint);
                    if (needRequestPressureParams)
                    {
                        _pressureParamsCheckPoints.Add(pressureParamsCheckPoint);
                    }
                }
                
                context.AddOrUpdate(
                    new CheckPointCycleProcessingContextParams(
                        needRequestEcg,
                        needRequestCommonParams,
                        needRequestPressureParams));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.Unknown,
                            e.Message,
                            e)));
            }
            
            return context;
        }

        public bool CanProcess([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
        
        private class IterationBasedCheckPoint
        {
            public IterationBasedCheckPoint(
                short currentCycleNumber, 
                short currentIterationNumber)
            {
                CurrentCycleNumber = currentCycleNumber;
                CurrentIterationNumber = currentIterationNumber;
            }

            private short CurrentCycleNumber { get; }
            
            private short CurrentIterationNumber { get; }

            private bool Equals(IterationBasedCheckPoint other)
            {
                return CurrentCycleNumber == other.CurrentCycleNumber && CurrentIterationNumber == other.CurrentIterationNumber;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((IterationBasedCheckPoint) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (CurrentCycleNumber.GetHashCode() * 397) ^ CurrentIterationNumber.GetHashCode();
                }
            }
        }
    }
}