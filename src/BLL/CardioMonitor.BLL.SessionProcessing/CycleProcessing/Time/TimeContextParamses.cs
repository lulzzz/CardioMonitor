using System;
using Enexure.MicroBus.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.Time
{
    internal class TimeContextParamses: IContextParams
    {
        public static readonly Guid TypeId = new Guid("91941aeb-69ea-4955-b533-13b717a4768d");
        
        public Guid ParamsTypeId { get; } = TypeId;

        public TimeContextParamses(
            TimeSpan cycleDuration, 
            TimeSpan elapsedTime)
        {
            CycleDuration = cycleDuration;
            ElapsedTime = elapsedTime;
        }

        /// <summary>
        /// Длительность цикла
        /// </summary>
        public TimeSpan CycleDuration { get; }

        /// <summary>
        /// Прошедшее время
        /// </summary>
        public TimeSpan ElapsedTime { get; }
        
        /// <summary>
        /// Оставшееся время
        /// </summary>
        public TimeSpan RemainingTime => CycleDuration - ElapsedTime;
    }

    internal static class TimeParamContextExtensions
    {
        public static TimeContextParamses TryGetTimeParams([JetBrains.Annotations.NotNull] [NotNull] this PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(TimeContextParamses.TypeId) as TimeContextParamses;
        }
    }
}