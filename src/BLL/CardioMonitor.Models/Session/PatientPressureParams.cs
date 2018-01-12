using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// специальная сущность для получения параметров пациента, связанных с предварительной накачкой давления
    /// </summary>
    public class PatientPressureParams
    {
        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public short SystolicArterialPressure { get; set; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public short DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public short AverageArterialPressure { get; set; }
    }
}
