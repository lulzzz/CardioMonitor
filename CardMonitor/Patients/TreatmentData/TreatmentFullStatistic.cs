﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Patients.TreatmentData
{
    public class TreatmentFullStatistic
    {
        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public TreatmentParamStatistic HeartRate { get; set; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        /// <remarks>Я правильно расшифровал?</remarks>
        public TreatmentParamStatistic RepsirationRate { get; set; }

        /// <summary>
        /// SPO2
        /// </summary>
        public TreatmentParamStatistic Spo2 { get; set; }

        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public TreatmentParamStatistic SystolicArterialPressure { get; set; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public TreatmentParamStatistic DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public TreatmentParamStatistic AverageArterialPressure { get; set; }
    }
}
