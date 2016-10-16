using System;
using MahApps.Metro.Controls;

namespace CardioMonitor.Threading
{
    /// <summary>
    /// Помощник для работы с потоками
    /// </summary>
    public class ThreadAssistant
    {
        private readonly MetroWindow _metroWindow;

        /// <summary>
        /// Помощник для работы с потоками
        /// </summary>
        public ThreadAssistant(MetroWindow metriWindow)
        {
            _metroWindow = metriWindow;
        }

        /// <summary>
        /// Выполняет переданный метод в потоке UI пользователя
        /// </summary>
        /// <param name="method">Метод</param>
        public void StartInUiThread(Action method)
        {
            //TODO Можно заменить реализацию, вызывать application.currentWindow
            _metroWindow.Dispatcher.BeginInvoke(method);
        }

    }
}
