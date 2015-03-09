using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;

namespace CardioMonitor.Core
{
    public sealed class MessageHelper
    {
        private static volatile MessageHelper _instance;
        private static readonly object _syncObject = new object();
        private MetroWindow _window;

        private MessageHelper()
        {
        }

        public static MessageHelper Instance
        {
            get
            {
                if (null != _instance) { return _instance;}
                lock (_syncObject)
                {
                    if (null == _instance)
                    {
                        _instance = new MessageHelper();
                    }
                }
                return _instance;
            }
        }

        public MetroWindow Window
        {
            private get { return _window; }
            set
            {
                lock (_syncObject)
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
