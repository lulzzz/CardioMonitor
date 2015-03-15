using System;
using CardioMonitor.Patients.Session;
using MySql.Data.MySqlClient;

namespace CardioMonitor.Core.Repository
{
    public class SafeReader
    {
        readonly MySqlDataReader _reader;

        public SafeReader(MySqlDataReader reader)
        {
            _reader = reader;
        }

        public string SafeGetString(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetString(colIndex) : string.Empty;
        }

        public int SafeGetInt(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetInt32(colIndex) : 0;
        }

        public double SafeGetDouble(int columndIndex)
        {
            return  !_reader.IsDBNull(columndIndex) ?_reader.GetDouble(columndIndex) :0;
        }

        public DateTime SafeGetTime(int colIndex)
        {
            return !_reader.IsDBNull(colIndex) ? _reader.GetDateTime(colIndex) : new DateTime(0001, 01, 01);
        }

        public SessionStatus SafeGetGender(int colIndex)
        {
            return (SessionStatus)SafeGetInt(colIndex);
        }
    }
}
