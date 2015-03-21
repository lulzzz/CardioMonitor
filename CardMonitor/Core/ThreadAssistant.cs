using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace CardioMonitor.Core
{
    /// <summary>
    /// Помощник для работы с потоками
    /// </summary>
    public class ThreadAssistant
    {
        private readonly MetroWindow _userControl;

        /// <summary>
        /// Помощник для работы с потоками
        /// </summary>
        public ThreadAssistant(MetroWindow userControl)
        {
            _userControl = userControl;
        }

        /// <summary>
        /// Выполняет переданный метод в потоке UI пользователя
        /// </summary>
        /// <param name="method"></param>
        public void StartInUIThread(Action method)
        {
            _userControl.Dispatcher.BeginInvoke(method);
        }

    }
}
