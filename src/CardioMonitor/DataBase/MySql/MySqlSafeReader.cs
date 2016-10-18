using System;
using CardioMonitor.Models.Session;
using MySql.Data.MySqlClient;

namespace CardioMonitor.DataBase.MySql
{
    /// <summary>
    /// Класс для безопасного чтения данных из результата SQL запроса 
    /// </summary>
    /// <remarks>
    /// В случае успеха возвращает запрашиваемое значение, если такой ячейки нет,
    /// то будет возвращено некоторое значение по умолчанию, 
    /// исключения выбрасываться не будут
    /// </remarks>
    internal class MySqlSafeReader : ISafeReader
    {
        readonly MySqlDataReader _reader;

        /// <summary>
        /// Класс для безопасного чтения данных из результата SQL запроса 
        /// </summary>
        /// <param name="reader">MySqlDataReader с результатом SQL запроса</param>
        public MySqlSafeReader(MySqlDataReader reader)
        {
            _reader = reader;
        }

        public bool CanRead()
        {
            return _reader.Read();
        }

        /// <summary>
        /// Возвращает строку
        /// </summary>
        /// <param name="colIndex">Номер колонки</param>
        /// <returns></returns>
        public string GetString(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetString(colIndex) : string.Empty;
        }

        /// <summary>
        /// Возвращает целое число
        /// </summary>
        /// <param name="colIndex">Номер колонки</param>
        /// <returns></returns>
        public int GetInt(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetInt32(colIndex) : 0;
        }

        /// <summary>
        /// Возвращает дробное число
        /// </summary>
        /// <param name="columnIndex">Номер колонки</param>
        /// <returns></returns>
        public double GetDouble(int columnIndex)
        {
            return  !_reader.IsDBNull(columnIndex) ?_reader.GetDouble(columnIndex) :0;
        }

        /// <summary>
        /// Возвращает объект DateTime
        /// </summary>
        /// <param name="colIndex">Номер колонки</param>
        /// <returns></returns>
        public DateTime GetDateTime(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetDateTime(colIndex) : new DateTime(0001, 01, 01);
        }

        public DateTime? GetNullableDateTime(int colIndex)
        {
            var dateTime = !_reader.IsDBNull(colIndex) ? _reader.GetDateTime(colIndex) : new DateTime(0001, 01, 01);
            return (dateTime.Equals(new DateTime(0001, 01, 01))) ? null: new DateTime?(dateTime);

        }
        
        /// <summary>
        /// Возвращает статус сессии
        /// </summary>
        /// <param name="colIndex">Номер колонки</param>
        /// <returns></returns>
        public SessionStatus GetSessionStatus(int colIndex)
        {
            return (SessionStatus)GetInt(colIndex);
        }
    }
}
