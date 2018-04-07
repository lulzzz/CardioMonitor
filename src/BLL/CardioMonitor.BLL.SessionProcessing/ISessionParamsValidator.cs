namespace CardioMonitor.BLL.SessionProcessing
{
    public interface ISessionParamsValidator
    {
        bool IsCyclesCountValid(short cyclesCount);
        bool IsMaxXAngleValid(float maxXAngle);
        bool IsMovementFrequencyValid(float movementFrequency);
        bool IsPumpingNumberOfAttemptsOnProcessing(short pumpingNumberOfAttempts);
        bool IsPumpingNumberOfAttemptsOnStartAndFinishValid(short pumpingNumberOfAttempts);
    }
}