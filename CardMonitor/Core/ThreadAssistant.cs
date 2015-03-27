using System;
using MahApps.Metro.Controls;

namespace CardioMonitor.Core
{
    /// <summary>
    /// Помощник для работы с потоками
    /// </summary>
    public class ThreadAssistant
    {
        private readonly MetroWindow _metriWindow;

        /// <summary>
        /// Помощник для работы с потоками
        /// </summary>
        public ThreadAssistant(MetroWindow metriWindow)
        {
            _metriWindow = metriWindow;
        }

        /// <summary>
        /// Выполняет переданный метод в потоке UI пользователя
        /// </summary>
        /// <param name="method">Метод</param>
        public void StartInUIThread(Action method)
        {
            _metriWindow.Dispatcher.BeginInvoke(method);
        }

    }
}
