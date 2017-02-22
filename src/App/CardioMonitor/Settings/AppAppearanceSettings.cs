using System.Windows.Media;

namespace CardioMonitor.Settings
{
    /// <summary>
    /// Настройки внешнего вида
    /// </summary>
    /// <remarks>Временно не используются</remarks>
    public class AppAppearanceSettings
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }
    }
}
