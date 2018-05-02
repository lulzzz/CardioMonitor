using System;


namespace CardioMonitor.Devices.Bed.Infrastructure
{
    /// <summary>
    /// класс для расчетов углов, периодов и времени исходя из заданных начальных значений
    /// </summary>
    public class BedSessionInfo
    {

        /// <summary>
        /// рассчет продолжительности одного цикла
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetCycleDuration()
        {
            return new TimeSpan(); //todo 
        }

        /// <summary>
        /// рассчет количества итераций в одном цикле
        /// </summary>
        /// <returns></returns>
        public short GetIterationsCount(double maxAngle)
        {
            return (short)Math.Round(maxAngle / 1.5 * 2 - 3); 
        }

        public short GetNextIterationNumberForPressureMeasuring()
        {
            return 0; //todo 
        }

        public short GetNextIterationNumberForCommonParamsMeasuring()
        {
            return 1; //todo 
        }

        public short GetNextIterationNumberForEcgMeasuring()
        {
            return 0; //todo 
        }
    }
}