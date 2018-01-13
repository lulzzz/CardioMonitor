using System;

namespace CardioMonitor.BLL.SessionProcessing.Exceptions
{
    /// <summary>
    /// Базовое исключение, которое может произойти при обработке сеанса
    /// </summary>
    public class SessionProcessingException : Exception
    {
        public SessionProcessingErrorCodes ErrorCode { get; }

        public SessionProcessingException(SessionProcessingErrorCodes errorCode)
        {
            ErrorCode = errorCode;
        }

        public SessionProcessingException(SessionProcessingErrorCodes errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public SessionProcessingException(SessionProcessingErrorCodes errorCode,string message, Exception exception) : base(message, exception)
        {
            ErrorCode = errorCode;
        }
    }
}