using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.Ui
{
    public sealed class MessageHelper
    {
        private static volatile MessageHelper _instance;
        private static readonly object SyncObject = new object();
        private MetroWindow _window;

        private MessageHelper()
        {
        }

        public static MessageHelper Instance
        {
            get
            {
                if (null != _instance)
                {
                    return _instance;
                }

                lock (SyncObject)
                {
                    return _instance ?? (_instance = new MessageHelper());
                }
            }
        }

        public MetroWindow Window
        {
            private get { return _window; }
            set
            {
                lock (SyncObject)
                {
                    if (_window != value || null != value)
                    {
                        _window = value;
                    }
                }
            }
        }

        public async Task<MessageDialogResult> ShowMessageAsync(string message, string title = null,
            MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            var result = await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                title = title ?? Window.Title;
                return await Window.ShowMessageAsync(title, message, style, settings);
            }).Task;
            return await result;
        }

        public async Task<ProgressDialogController> ShowProgressDialogAsync(string message, string title = null)
        {
            var result = await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                title = title ?? Window.Title;
                return await Window.ShowProgressAsync(title, message);
            }).Task;
            return await result;
        }
    }
}
