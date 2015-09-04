using System;
using System.Threading.Tasks;
using CardioMonitor.Infrastructure.Logs;

namespace CardioMonitor.Core.Threading
{
    /// <summary>
    /// Вспомогательный класс для работы с Task
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// Запускает задачу, которая должна выполниться за определенное время,
        /// и в случае истечения таймаута генерирует TimeoutException
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">Задача, которую следует запустить</param>
        /// <param name="timeout">Значение таймаута</param>
        /// <returns>Выполнившуюся задачу</returns>
        /// <exception cref="TimeoutException">В случае невыполнения задачу за указанный временный промежуток</exception>
        public static async Task<T> StartWithTimeout<T>(Task<T> task, TimeSpan timeout)
        {
            var delayTask = Task.Delay(timeout);
            var firstToFinish = await Task.WhenAny(task, delayTask);

            if (firstToFinish == delayTask)
            {
                // Если задачу ответить в течение указанного времени, значит она не зависла и можно
                // обработать исключения, если они возникли, иначе бросаем исключение
                var waitingTimeout = new TimeSpan(0, 0, 1);
                if (task.Wait(waitingTimeout))
                {
                    await task.ContinueWith(LogException);
                }
                //TODO нужно придумать, как убивать задачу
                throw new TimeoutException();
            }

            return await task;
        }

        /// <summary>
        /// Создает запись в лог об исключении
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">Задача, в которой, вероятно, могло быть исключение</param>
        private static void LogException<T>(Task<T> task)
        {
            if (task.Exception != null)
            {
                Logger.Instance.LogError("TaskHelper", task.Exception);
            }
        }
    }
}
