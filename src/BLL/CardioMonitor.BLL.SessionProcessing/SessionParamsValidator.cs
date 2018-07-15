using System;
using System.Collections.Generic;
using CardioMonitor.Infrastructure;

namespace CardioMonitor.BLL.SessionProcessing
{
    public class SessionParamsValidator : ISessionParamsValidator
    {
        public bool IsCyclesCountValid(short cyclesCount)
        {
            return SessionParamsConstants.MinCyclesCount <= cyclesCount
                   && cyclesCount <= SessionParamsConstants.MaxCyclesCount;
        }

        public bool IsMaxXAngleValid(float maxXAngle)
        {
            if (SessionParamsConstants.MinValueMaxXAngle > maxXAngle
                || maxXAngle > SessionParamsConstants.MaxValueMaxXAngle) return false;

            var availableValues = new HashSet<double>();
            for (var availableValue = SessionParamsConstants.MinValueMaxXAngle; availableValue <= SessionParamsConstants.MaxValueMaxXAngle; availableValue += SessionParamsConstants.MaxXAngleStep)
            {
                availableValues.Add(availableValue);
            }

            return availableValues.Contains(maxXAngle);
        }

        public bool IsMovementFrequencyValid(float movementFrequency)
        {
            return SessionParamsConstants.MinMovementFrequency <= movementFrequency
                   && movementFrequency <= SessionParamsConstants.MaxMovementFrequency;
        }

        public bool IsPumpingNumberOfAttemptsOnStartAndFinishValid(short pumpingNumberOfAttempts)
        {
            return SessionParamsConstants.MinPumpingNumberOfAttemptsOnStartAndFinish <= pumpingNumberOfAttempts
                   && pumpingNumberOfAttempts <= SessionParamsConstants.MaxPumpingNumberOfAttemptsOnStartAndFinish;
        }

        public bool IsPumpingNumberOfAttemptsOnProcessing(short pumpingNumberOfAttempts)
        {
            return SessionParamsConstants.MinPumpingNumberOfAttemptsOnProcessing <= pumpingNumberOfAttempts
                   && pumpingNumberOfAttempts <= SessionParamsConstants.MaxPumpingNumberOfAttemptsOnProcessing;
        }
    }
}