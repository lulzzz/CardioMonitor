using System;
using CardioMonitor.Core.Models.Treatment;
using CardioMonitor.Patients.TreatmentData;

namespace CardioMonitor.Core.Repository
{
    public class TreatmentStatisticBuilder
    {
        public TreatmentFullStatistic Build(Core.Models.Session.Session[] sessions)
        {
            if (sessions == null) throw new ArgumentNullException("sessions");

            var heartRate = new TreatmentParamStatistic {Name = "ЧСС"};
            var repsirationRate = new TreatmentParamStatistic { Name = "ЧД" };
            var spo2 = new TreatmentParamStatistic { Name = "SPO2" };
            var systolicArterialPressure = new TreatmentParamStatistic { Name = "Систолическое АД" };
            var diastolicArterialPressure = new TreatmentParamStatistic { Name = "Диастолическое АД" };
            var averageArterialPressure = new TreatmentParamStatistic { Name = "Среднее АД" };



            foreach (var session in sessions)
            {
                foreach (var param in session.PatientParams)
                {
                    heartRate.AddStatisticPart(param.Iteraton, param.InclinationAngle,param.HeartRate);
                    repsirationRate.AddStatisticPart(param.Iteraton,param.InclinationAngle, param.RepsirationRate);
                    spo2.AddStatisticPart(param.Iteraton, param.InclinationAngle, param.Spo2);
                    systolicArterialPressure.AddStatisticPart(param.Iteraton, param.InclinationAngle, param.SystolicArterialPressure);
                    diastolicArterialPressure.AddStatisticPart(param.Iteraton, param.InclinationAngle, param.DiastolicArterialPressure);
                    averageArterialPressure.AddStatisticPart(param.Iteraton, param.InclinationAngle, param.AverageArterialPressure);
                }
            }

            return new TreatmentFullStatistic
            {
                HeartRate =  heartRate,
                RepsirationRate =  repsirationRate,
                Spo2 = spo2,
                SystolicArterialPressure = systolicArterialPressure,
                DiastolicArterialPressure = diastolicArterialPressure,
                AverageArterialPressure = averageArterialPressure
            };
        }
    }
}
