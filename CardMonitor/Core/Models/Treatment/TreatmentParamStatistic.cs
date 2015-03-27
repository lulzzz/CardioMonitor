using System;
using System.Collections.Generic;
using System.Linq;

namespace CardioMonitor.Core.Models.Treatment
{
    //todo Временно не используется
    public class TreatmentParamStatistic
    {
        private const double Tolerance = 0.00001;

        public string Name { get; set; }

        public List<Statistic> Statistics { get; set; }

        public TreatmentParamStatistic()
        {
            Statistics = new List<Statistic>();
        }

        public void AddStatisticPart(int iteration, double inclinationAngle, int value)
        {
            var statistic = Statistics.FirstOrDefault(x => x.Iteration == iteration && Math.Abs(x.InclinationAngle - inclinationAngle) < Tolerance) ??
                            new Statistic
            {
                InclinationAngle = inclinationAngle,
                Iteration = iteration,
                Values = new List<int>()
            };
            statistic.Values.Add(value);
        }
    }
}
