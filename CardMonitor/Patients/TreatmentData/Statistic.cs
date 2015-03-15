using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Patients.TreatmentData
{
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
