using System;
using CardioMonitor.Models.Session;

namespace CardioMonitor.DataBase
{
    public interface ISafeReader
    {
        bool CanRead();

        /// <summary>
        /// Возвращает строку
        /// </summary>
        /// <param name="colIndex">Номер колонки</param>
        /// <returns></returns>
        string GetString(int colIndex);

        /// <summary>
        /// Возвращает целое число
        /// </summary>
        /// <param name="colIndex">Номер колонки</param>
        /// <returns></returns>
        int GetInt(int colIndex);

        /// <summary>
        /// Возвращает дробное число
        /// </summary>
        /// <param name="columnIndex">Номер колонки</param>
        /// <returns></returns>
        double GetDouble(int columnIndex);


        /// <summary>
        /// Возвращает объект DateTime
        /// </summary>
        /// <param name="colIndex">Номер колонки</param>
        /// <returns></returns>
        DateTime GetDateTime(int colIndex);

        DateTime? GetNullableDateTime(int colIndex);

        SessionStatus GetSessionStatus(int colIndex);
    }
}