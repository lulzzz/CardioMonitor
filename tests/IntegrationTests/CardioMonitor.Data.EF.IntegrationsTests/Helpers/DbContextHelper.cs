using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Data.Ef.Context;

namespace CardioMonitor.Dal.Ef.UnitTests.Helpers
{
    class DbContextHelper
    {
        public static CardioMonitorContext GetInitializeContext()
        {
            //todo init
            var context = new CardioMonitorContext();

            return context;
        }

        public static void DropDb()
        {
            //todo drop all
        }
    }
}
