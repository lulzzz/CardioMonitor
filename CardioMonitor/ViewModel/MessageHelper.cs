using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.ViewModel
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

        public Task<MessageDialogResult> ShowMessageAsync(string message, string title = null , MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            title = title ?? Window.Title;
            return Window.ShowMessageAsync(title, message, style, settings);
        }
    }
}
