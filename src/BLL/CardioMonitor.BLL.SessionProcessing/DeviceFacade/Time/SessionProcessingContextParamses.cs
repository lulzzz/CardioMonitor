using System;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using Enexure.MicroBus.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time
{
    internal class SessionProcessingContextParamses: ICycleProcessingContextParams
    {
        public static readonly Guid TypeId = new Guid("91941aeb-69ea-4955-b533-13b717a4768d");
        
        public Guid ParamsTypeId { get; } = TypeId;

        public SessionProcessingContextParamses(
            TimeSpan cycleDuration, 
            TimeSpan elapsedTime, 
            TimeSpan remainingTime, 
            short currentCycleNumber, 
            short cyclesCount)
        {
            CycleDuration = cycleDuration;
            ElapsedTime = elapsedTime;
            RemainingTime = remainingTime;
            CurrentCycleNumber = currentCycleNumber;
            CyclesCount = cyclesCount;
        }

        /// <summary>
        /// Длительность цикла
        /// </summary>
        public TimeSpan CycleDuration { get; }

        /// <summary>
        /// Прошедшее время с начала сеанса
        /// </summary>
        public TimeSpan ElapsedTime { get; }
        
        /// <summary>
        /// Оставшееся время с начала сеанса
        /// </summary>
        public TimeSpan RemainingTime { get; }
        
        public short CurrentCycleNumber { get; }
        
        public short CyclesCount { get; }
        
    }

    internal static class TimeParamContextExtensions
    {
        public static SessionProcessingContextParamses TryGetTimeParams([JetBrains.Annotations.NotNull] [NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(SessionProcessingContextParamses.TypeId) as SessionProcessingContextParamses;
        }
    }
}