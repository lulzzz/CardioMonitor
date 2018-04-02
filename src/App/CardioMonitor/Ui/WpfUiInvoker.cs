using System;
using System.Threading.Tasks;
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

        public T Invoke<T>(Func<T> function)
        {
            return Application.Current.Dispatcher.Invoke(function);
        }

        public Task Invoke(Func<Task> function)
        {
            return Application.Current.Dispatcher.Invoke(function);
        }

        public Task InvokeAsync(Action action)
        {
            return Application.Current.Dispatcher.InvokeAsync(action).Task;
        }

        public Task InvokeAsync(Func<Task> function)
        {
            return Application.Current.Dispatcher.InvokeAsync(function).Task;
        }
    }
}