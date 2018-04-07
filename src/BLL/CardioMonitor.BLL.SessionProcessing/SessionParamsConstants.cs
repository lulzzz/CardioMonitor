namespace CardioMonitor.BLL.SessionProcessing
{
    public static class SessionParamsConstants
    {
        public static readonly short DefaultCyclesCount = 1;

        public static readonly short MinCyclesCount = 1;

        public static readonly short MaxCyclesCount = 40;


        public static readonly float DefaultMaxXAngle = 31.5f;

        public static readonly float MinValueMaxXAngle = 7.5f;

        public static readonly float MaxValueMaxXAngle = 31.5f;

        public static readonly float MaxXAngleStep = 1.5f;
        

        public static readonly float DefaultMovementFrequency = 0.1f;

        public static readonly float MinMovementFrequency = 0.07f;

        public static readonly float MaxMovementFrequency = 0.145f;

        public static readonly float MovementFrequencyStep = 0.005f;

        public static readonly short DefaultPumpingNumberOfAttemptsOnStartAndFinish = 2;

        public static readonly short MinPumpingNumberOfAttemptsOnStartAndFinish = 0;

        public static readonly short MaxPumpingNumberOfAttemptsOnStartAndFinish = 4;


        public static readonly short DefaultPumpingNumberOfAttemptsOnProcessing = 1;

        public static readonly short MinPumpingNumberOfAttemptsOnProcessing = 0;

        public static readonly short MaxPumpingNumberOfAttemptsOnProcessing = 4;
    }
}