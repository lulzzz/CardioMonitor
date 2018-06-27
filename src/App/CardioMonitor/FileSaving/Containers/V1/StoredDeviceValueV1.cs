using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.FileSaving.Containers.V1
{
    /// <summary>
    /// Значение, полученное с внешнего устройства
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct StoredDeviceValueV1<T>
    {
        public StoredDeviceValueStatusV1 Status { get; set; }

        /// <summary>
        /// Данные, полученные от устройства
        /// </summary>
        public T Value { get; set; }
        
    }
}
