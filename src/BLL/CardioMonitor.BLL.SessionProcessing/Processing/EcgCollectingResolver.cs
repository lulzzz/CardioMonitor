using System;
using System.Linq;
using JetBrains.Annotations;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Определитель необходимости измерения ЭКГ во время сеанса
    /// </summary>
    /// <remarks>
    /// Т.е. в максимальной точке (по крайней мере, сейчас), определение на старте и завершении работает по другому принципу
    /// </remarks>
    public class EcgCollectingResolver
    {
        private double[] ecgCollectingPointAngles { get; }

        public EcgCollectingResolver([NotNull] double[] checkPointAngles)
        {
            if (checkPointAngles == null) throw new ArgumentNullException(nameof(checkPointAngles));
            if (checkPointAngles.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(checkPointAngles));
            
            ecgCollectingPointAngles = new[]
            {
                checkPointAngles.Max()
            };
        }
    }
}