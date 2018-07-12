using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Settings
{
    /// <summary>
    /// Ошибка настроек
    /// </summary>
    /// <remarks>
    /// Например, поле на задано
    /// </remarks>
    internal class SettingsException : Exception
    {
        public SettingsException()
        {
            
        }

        public SettingsException(string message)
            : base(message)
        {
            
        }
    }
}
