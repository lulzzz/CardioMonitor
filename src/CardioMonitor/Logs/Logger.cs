using System;
using System.IO;

// ReSharper disable UnassignedField.Compiler
// ReSharper disable LocalizableElement
namespace CardioMonitor.Logs
{
    /// <summary>
    /// Класс для записи лог-информации в файл
    /// </summary>
    public class Logger
    {
        private readonly string _logsFolder;
        private static Logger _instance;
        private static readonly object LockObject = new object();

        /// <summary>
        /// Закрытй конструктор
        /// </summary>
        private Logger()
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
        /// Класс для записи лог-информации в файл
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (null != _instance)
                {
                    return _instance;
                }
                lock (LockObject)
                {
                    return _instance ?? (_instance = new Logger());
                }
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
                var logMessage = String.Format("{0} {1}\nException info: \n{2}\n------------\n", className, DateTime.Now.ToShortTimeString(), exceptionInfo);
                await textFile.WriteAsync(logMessage);
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
                var logMessage = String.Format("{0} {1} \nMessage: \n{2} \nSource: \n{3} \nStackTrace: \n{4}\n------------\n", className, DateTime.Now.ToShortTimeString(), ex.Message, ex.Source, ex.StackTrace);
                await textFile.WriteAsync(logMessage);
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
                var logMessage = String.Format("{0} \nException info: \n{1}\n------------\n", DateTime.Now.ToShortTimeString(), query);
                await textFile.WriteAsync(logMessage);
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
                var logMessage = String.Format("{0} \nInfo: \n{1}\n------------\n", DateTime.Now.ToShortTimeString(), message);
                await textFile.WriteAsync(logMessage);
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