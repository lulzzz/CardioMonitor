using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// ЭКГ пациента
    /// </summary>
    public class PatientECG
    {
        /// <summary>
        /// Массив значений ЭКГ пациента
        /// по ТЗ требуется 10 секунд при частоте 375 Гц == 3750 точек
        /// значения емнип 0 - 2047 (у координата)
        /// </summary>
        public int[] ECG { get; set; }
    }
}
