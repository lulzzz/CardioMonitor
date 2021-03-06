﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CardioMonitor.BLL.CoreContracts.Session;

namespace CardioMonitor.Ui.Converters
{
    public class NotStartedSessionStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = SessionStatus.NotStarted;

            if (value is SessionStatus sessionStatus)
            {
                status = sessionStatus;
            }

            return (status == SessionStatus.NotStarted)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}