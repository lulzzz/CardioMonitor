using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Core.Repository.BedController
{
    /// <summary>
    /// Флаг старт
    /// </summary>
    public enum StartFlag
    {
        /// <summary>
        /// Начальное состояние
        /// </summary>
        Default = -1,
        /// <summary>
        /// Пауза
        /// </summary>
        Pause = 0,
        /// <summary>
        /// Старт
        /// </summary>
        Start = 1
    }
}
