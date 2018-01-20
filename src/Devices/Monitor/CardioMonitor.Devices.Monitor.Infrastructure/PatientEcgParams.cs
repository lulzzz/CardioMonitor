using System;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    /// <summary>
    /// ЭКГ пациента
    /// </summary>
    public class PatientEcgParams
    {
        public PatientEcgParams([NotNull] short[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Массив значений ЭКГ пациента
        /// по ТЗ требуется 10 секунд при частоте 375 Гц == 3750 точек
        /// значения емнип 0 - 2047 (у координата)
        /// </summary>
        public short[] Data { get;  }
        
    }
}