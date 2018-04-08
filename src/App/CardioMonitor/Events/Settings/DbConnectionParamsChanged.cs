

namespace CardioMonitor.Events.Settings
{
    public class DbConnectionParamsChanged 

    {
    public DbConnectionParamsChanged(string connenctionString)
    {
        ConnenctionString = connenctionString;
    }

    public string ConnenctionString { get; }
    }
}