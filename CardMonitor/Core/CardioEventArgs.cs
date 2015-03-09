using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Core
{
    public class CardioEventArgs : EventArgs
    {
        public int Id { get; set; }

        public CardioEventArgs(int id)
        {
            Id = id;
        }
    }
}
