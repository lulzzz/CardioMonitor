using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CardioMonitor.BLL.CoreContracts.Session;

namespace CardioMonitor.Ui.Converters
{
    public class SessionStatusToSaveToFileButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = SessionStatus.NotStarted;

            if (value is SessionStatus sessionStatus)
            {
                status = sessionStatus;
            }

            return (status == SessionStatus.Completed
                    || status == SessionStatus.EmergencyStopped
                    || status == SessionStatus.TerminatedOnError)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}