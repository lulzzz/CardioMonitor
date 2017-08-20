namespace CardioMonitor.SessionProcessing.StateMachine
{
    public enum CycleStates
    {
        NotStarted,
        InProgress,
        Suspedned,
        Reversed, 
        EmergencyStopped,
        Completed
    }

    public enum SessionStates
    {
        NotStarted,
        InProgress,
        Susspened,
        EmergencyStopped,
        Completed
    }
}