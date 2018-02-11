namespace CardioMonitor.BLL.SessionProcessing.Exceptions
{
    public enum SessionProcessingErrorCodes
    {
        Unknown=0,
        UnhandledException=1,
        PumpingTimeout=2,
        PumpingError=3,
        PatientPressureParamsRequestError=4,
        PatientPressureParamsRequestTimeout=5,
        PatientCommonParamsRequestError=6,
        PatientCommonParamsRequestTimeout=7,
        MonitorConnectionError =8,
        MonitorProcessingError =9,
        InversionTableConnectionError=10,
        InversionTableProcessingError=11,
        UpdateAngleError = 11,
        StartFailed=12
    }
}