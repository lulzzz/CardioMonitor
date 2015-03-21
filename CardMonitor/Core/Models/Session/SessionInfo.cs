using System;

namespace CardioMonitor.Core.Models.Session
{
    /// <summary>
    /// Краткая информация о сеансе
    /// </summary>
    public class SessionInfo
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public SessionStatus Status { get; set; }
    }
}
