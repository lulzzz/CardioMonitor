﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Patients.Session
{
    public class PatientParams
    {
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор сеанса лечения
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public double InlcinationAngle { get; set; }

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public int HeartRate { get; set; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        /// <remarks>Я правильно расшифровал?</remarks>
        public int RepsirationRate { get; set; }

        /// <summary>
        /// SPO2
        /// </summary>
        public int Spo2 { get; set; }

        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public int SystolicArterialPressure { get; set; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public int DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public int AverageArterialPressure { get; set; }
    }
}
