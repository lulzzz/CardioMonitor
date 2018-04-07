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
            return SessionParamsConstants.MinValueMaxXAngle <= maxXAngle
                   && maxXAngle <= SessionParamsConstants.MaxValueMaxXAngle;
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