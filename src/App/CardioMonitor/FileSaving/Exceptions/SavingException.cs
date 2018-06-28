using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.FileSaving.Exceptions
{
    internal class SavingException : Exception
    {
        public SavingException()
        {
            
        }

        public SavingException(string message)
            : base(message)
        {
        }

        public SavingException(string message, Exception inner)
            : base(message, inner)
        {
            
        }
    }
}
