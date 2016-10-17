using System;

namespace CardioMonitor.DataBase
{
    /// <summary>
    /// Переводчик ошибок при работе с базой, который по исключению и коду ошибку возвращает понятную для приложения информацию
    /// </summary>
    public interface IDataBaseErrorTranslator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        DataBaseError Translate(int errorCode);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        DataBaseError Translate(Exception exception);
    }
}