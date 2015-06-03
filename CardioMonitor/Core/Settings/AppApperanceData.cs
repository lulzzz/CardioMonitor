using System.Windows.Media;

namespace CardioMonitor.Core.Settings
{
    /// <summary>
    /// Настройки внешнего вида
    /// </summary>
    /// <remarks>Временно не используются</remarks>
    public class AppApperanceData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }
    }
}
