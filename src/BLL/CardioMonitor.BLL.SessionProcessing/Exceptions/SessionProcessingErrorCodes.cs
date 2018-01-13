namespace CardioMonitor.BLL.SessionProcessing.Exceptions
{
    public enum SessionProcessingErrorCodes
    {
        Unknown=0,
        UnhandlerException=1,
        PumpingTimeout=2,
        PumpingError=3,
        PatientPressureParamsRequestError=4,
        PatientPressureParamsRequestTimeout=5,
        PatientCommonParamsRequestError=6,
        PatientCommonParamsRequestTimeout=7,
        MonitorConnectionError =8,
        InversionTableConnectionError=9,
        UpdateAngleError = 10
    }
}