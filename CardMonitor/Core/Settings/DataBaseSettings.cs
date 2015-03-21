using System;

namespace CardioMonitor.Settings
{
    [Serializable]
    public class DataBaseSettings
    {
        public string DataBase { get; set; }
        public string Source { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
