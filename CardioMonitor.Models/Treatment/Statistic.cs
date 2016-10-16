using System.Collections.Generic;

namespace CardioMonitor.Models.Treatment
{
    //todo Временно не используется
    public class Statistic
    {
        public int Iteration { get; set; }

        public double InclinationAngle { get; set; }

        public List<int> Values { get; set; }

        public Statistic()
        {
            Values =  new List<int>();
        }
    }
}
