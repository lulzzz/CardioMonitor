using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CardioMonitor.Infrastructure.WpfCommon.Converters
{
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool b)
                flag = b;

            return (Visibility)(flag ? 2 : 0);
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Collapsed;
            return true;
        }
    }
}