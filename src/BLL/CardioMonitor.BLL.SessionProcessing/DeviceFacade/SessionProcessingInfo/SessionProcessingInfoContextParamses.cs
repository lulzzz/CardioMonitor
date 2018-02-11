using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time
{
    internal class SessionProcessingInfoContextParamses: ICycleProcessingContextParams
    {
        public static readonly Guid TypeId = new Guid("91941aeb-69ea-4955-b533-13b717a4768d");
        
        public Guid ParamsTypeId { get; } = TypeId;
        public Guid UniqObjectId { get; }

        public SessionProcessingInfoContextParamses(
            TimeSpan elapsedTime, 
            TimeSpan remainingTime, 
            TimeSpan cycleDuration, 
            short currentCycleNumber, 
            short cyclesCount)
        {
            CycleDuration = cycleDuration;
            ElapsedTime = elapsedTime;
            RemainingTime = remainingTime;
            CurrentCycleNumber = currentCycleNumber;
            CyclesCount = cyclesCount;
            UniqObjectId = Guid.NewGuid();
        }

        /// <summary>
        /// Прошедшее время с начала сеанса
        /// </summary>
        public TimeSpan ElapsedTime { get; }
        
        /// <summary>
        /// Оставшееся время с начала сеанса
        /// </summary>
        public TimeSpan RemainingTime { get; }
        
        /// <summary>
        /// Длительность цикла
        /// </summary>
        public TimeSpan CycleDuration { get; }
        
        /// <summary>
        /// Номер текущего цикла
        /// </summary>
        public short CurrentCycleNumber { get; }
        
        /// <summary>
        /// Общее количество циклов
        /// </summary>
        public short CyclesCount { get; }
        

        
    }

    internal static class TimeParamContextExtensions
    {
        [CanBeNull]
        public static SessionProcessingInfoContextParamses TryGetSessionProcessingInfo([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(SessionProcessingInfoContextParamses.TypeId) as SessionProcessingInfoContextParamses;
        }
    }
}