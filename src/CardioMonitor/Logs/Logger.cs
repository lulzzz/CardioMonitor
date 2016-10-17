using System;
using System.IO;
using CardioMonitor.Infrastructure.Logs;

// ReSharper disable UnassignedField.Compiler
// ReSharper disable LocalizableElement
namespace CardioMonitor.Logs
{
    /// <summary>
    /// Класс для записи лог-информации в файл
    /// </summary>
    public class Logger : ILogger
    {
        private readonly string _logsFolder;

        /// <summary>
        /// Класс для записи лог-информации в файл
        /// </summary>
        public Logger()
        {
            _logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Settings.Settings.AppName, "Logs");
            try
            {
                if (!Directory.Exists(_logsFolder))
                {
                    Directory.CreateDirectory(_logsFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't create log folder in AppData. \n\t{0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Добавляет запись об ошибке в лог
        /// </summary>
        /// <param name="className">Названия класса, в котором произошла ошибка</param>
        /// <param name="exceptionInfo">Текст записи</param>
        public async void LogError(string className, string exceptionInfo)
        {
            try
            {
                var fileName = _logsFolder + @"\Error_" + DateTime.Today.Date.ToShortDateString() + ".log";
                var textFile = new StreamWriter(fileName, true);
                var logMessage =
                    $"{className} {DateTime.Now.ToShortTimeString()}\nException info: \n{exceptionInfo}\n------------\n";
                await textFile.WriteAsync(logMessage).ConfigureAwait(false);
                textFile.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't save log to file. \n\t{0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Добавляет запись об ошибке в лог
        /// </summary>
        /// <param name="className">Название класса, в котором произошла ошибка</param>
        /// <param name="ex">Ошибка, информацию о которой следует сохранить</param>
        public async void LogError(string className, Exception ex)
        {
            try
            {
                var fileName = _logsFolder + @"\Error_" + DateTime.Today.Date.ToShortDateString() + ".log";
                var textFile = new StreamWriter(fileName, true);
                var logMessage =
                    $"{className} {DateTime.Now.ToShortTimeString()} \nMessage: \n{ex.Message} \nSource: \n{ex.Source} \nStackTrace: \n{ex.StackTrace}\n------------\n";
                await textFile.WriteAsync(logMessage).ConfigureAwait(false);
                textFile.Close();
            }
            catch (Exception logEx)
            {
                Console.WriteLine("Can't save log to file. \n\t{0}\n\t{1}", logEx.Message, logEx.StackTrace);
            }
        }

        /// <summary>
        /// Добавляет в лог SQL запрос, который привел к ошибке 
        /// </summary>
        /// <param name="query">SQL запрос</param>
        public async void LogQueryError(string query)
        {
            try
            {
                var fileName = _logsFolder + @"\Querry_" + DateTime.Today.Date.ToShortDateString() + ".log";
                var textFile = new StreamWriter(fileName, true);
                var logMessage = $"{DateTime.Now.ToShortTimeString()} \nException info: \n{query}\n------------\n";
                await textFile.WriteAsync(logMessage).ConfigureAwait(false);
                textFile.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't save log to file. \n\t{0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        public async void Log(string message)
        {
            try
            {
                var fileName = _logsFolder + @"\Message_" + DateTime.Today.Date.ToShortDateString() + ".log";
                var textFile = new StreamWriter(fileName, true);
                var logMessage = $"{DateTime.Now.ToShortTimeString()} \nInfo: \n{message}\n------------\n";
                await textFile.WriteAsync(logMessage).ConfigureAwait(false);
                textFile.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't save log to file. \n\t{0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
// ReSharper restore UnassignedField.Compiler
// ReSharper restore LocalizableElement