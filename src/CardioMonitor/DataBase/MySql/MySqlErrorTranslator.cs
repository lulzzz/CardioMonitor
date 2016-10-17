using System;
using MySql.Data.MySqlClient;

namespace CardioMonitor.DataBase.MySql
{
    public class MySqlErrorTranslator : IDataBaseErrorTranslator
    {
        private const int HostError = 1042;
        private const int AccessDeniedError = 0;

        public DataBaseError Translate(int errorCode)
        {
            switch (errorCode)
            {
                case HostError:
                    return DataBaseError.HostError;
                case AccessDeniedError:
                    return DataBaseError.AccessDenied;
                default:
                    return DataBaseError.Unknown;
            }
        }

        public DataBaseError Translate(Exception exception)
        {
            var mysqlException = exception as MySqlException;
            if (mysqlException == null) return DataBaseError.Unknown;

            return Translate(mysqlException.ErrorCode);
        }
    }
}