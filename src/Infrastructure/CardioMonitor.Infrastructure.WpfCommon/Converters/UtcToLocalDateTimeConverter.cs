using System;
using System.Globalization;
using System.Windows.Data;

namespace CardioMonitor.Infrastructure.WpfCommon.Converters
{
    public class UtcToLocalDateTimeConverter : IValueConverter
    {
        public object Convert(
            object value, 
            Type targetType, 
            object parameter, 
            CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (DateTime.TryParse(value.ToString(), out var dateTime))
                throw new ArgumentException($"{nameof(value)} must be {typeof(DateTime)} type");

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
