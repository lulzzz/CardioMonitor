using System;
using CardioMonitor.Patients.Session;
using MySql.Data.MySqlClient;

namespace CardioMonitor.Core.Repository
{
    internal class SafeReader
    {
        readonly MySqlDataReader _reader;

        public SafeReader(MySqlDataReader reader)
        {
            _reader = reader;
        }

        public string GetString(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetString(colIndex) : string.Empty;
        }

        public int GetInt(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetInt32(colIndex) : 0;
        }

        public double GetDouble(int columndIndex)
        {
            return  !_reader.IsDBNull(columndIndex) ?_reader.GetDouble(columndIndex) :0;
        }

        public DateTime GetDateTime(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetDateTime(colIndex) : new DateTime(0001, 01, 01);
        }

        public SessionStatus GetSesionStatus(int colIndex)
        {
            return (SessionStatus)GetInt(colIndex);
        }
    }
}
