using System;
using System.Windows;
using Markeli.Storyboards;

namespace CardioMonitor.Ui
{
    public class WpfUiInvoker : IUiInvoker
    {
        public void Invoke(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }
    }
}