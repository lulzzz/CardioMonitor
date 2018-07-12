using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CardioMonitor.BLL.CoreContracts.Session;
using JetBrains.Annotations;

namespace CardioMonitor.Ui.Converters
{
    public class SessionStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!Enum.TryParse<SessionStatus>(value.ToString(), result: out var status, ignoreCase: true)) 
                throw new ArgumentException($"Converter supports only {typeof(SessionStatus)}");

            switch (status)
            {
                case SessionStatus.NotStarted:
                    return "Не начат";
                case SessionStatus.Completed:
                    return "Завершен";
                case SessionStatus.TerminatedOnError:
                    return "Ошибка сеанса";
                case SessionStatus.InProgress:
                    return "Выполняется";
                case SessionStatus.Suspended:
                    return "Приостановлен";
                case SessionStatus.EmergencyStopped:
                    return "Экстренно остановлен";
                default:
                    return "Неизвестен";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
