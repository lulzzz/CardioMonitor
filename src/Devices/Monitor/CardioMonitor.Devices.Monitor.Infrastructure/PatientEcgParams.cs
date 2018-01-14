namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    /// <summary>
    /// ЭКГ пациента
    /// </summary>
    public class PatientEcgParams
    {
        /// <summary>
        /// Массив значений ЭКГ пациента
        /// по ТЗ требуется 10 секунд при частоте 375 Гц == 3750 точек
        /// значения емнип 0 - 2047 (у координата)
        /// </summary>
        public int[] Data { get; set; }
        
    }
}