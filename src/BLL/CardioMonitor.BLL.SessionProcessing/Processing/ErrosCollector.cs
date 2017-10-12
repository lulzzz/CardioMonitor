using CardioMonitor.SessionProcessing.Events.Session;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Коллектор исключений, которые могу быть выброшены в разных частях обработки сеанса. 
    /// </summary>
    /// <remarks>
    /// Анализирует исключение и генериурет команду <see cref="EmergencyStopCommand"/> по необходимости
    /// </remarks>
    public class ErrosCollector
    {
        
    }
}