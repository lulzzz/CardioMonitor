using System;
using MySql.Data.MySqlClient;

namespace CardioMonitor.DataBase.MySql
{
    public class CardioMySqlDataReader : ISqlDataReader
    {
        public CardioMySqlDataReader(MySqlDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            Reader = reader;
        }

        public MySqlDataReader Reader { get; private set; }
    }
}