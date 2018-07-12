using System;

namespace CardioMonitor.BLL.SessionProcessing.Exceptions
{
    /// <summary>
    /// Базовое исключение, которое может произойти при обработке сеанса
    /// </summary>
    public class SessionProcessingException : Exception
    {
        public SessionProcessingErrorCodes ErrorCode { get; }
        
        /// <summary>
        /// Номер цикла
        /// </summary>
        public short? CycleNumber { get; }
        
        /// <summary>
        /// Номер итерации
        /// </summary>
        public short? IterationNumber { get; }

        public SessionProcessingException(
            SessionProcessingErrorCodes errorCode,
            short? cycleNumber = default(short?), 
            short? iterationNumber= default(short?))
        {
            ErrorCode = errorCode;
            CycleNumber = cycleNumber;
            IterationNumber = iterationNumber;
        }

        public SessionProcessingException(
            SessionProcessingErrorCodes errorCode,  
            string message,
            short? cycleNumber = default(short?), 
            short? iterationNumber= default(short?)) : base(message)
        {
            ErrorCode = errorCode;
            CycleNumber = cycleNumber;
            IterationNumber = iterationNumber;
        }

        public SessionProcessingException(
            SessionProcessingErrorCodes errorCode,
            string message, 
            Exception exception,
            short? cycleNumber = default(short?), 
            short? iterationNumber= default(short?)) : base(message, exception)
        {
            ErrorCode = errorCode;
            CycleNumber = cycleNumber;
            IterationNumber = iterationNumber;
        }
    }
}