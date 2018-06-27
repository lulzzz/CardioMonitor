using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.FileSaving.Containers.V1
{
    /// <summary>
    /// Статус показателя пациента, запрашиваемого с оборудования
    /// </summary>
    internal enum StoredDeviceValueStatusV1
    {
        /// <summary>
        /// Статус показателя неизвесен
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Показатель не запрашивался от оборудования
        /// </summary>
        NotObtained = 1,

        /// <summary>
        /// Значение показателя получено от оборудования
        /// </summary>
        Obtained = 2,

        /// <summary>
        /// Произошла ошибка запроса показателя
        /// </summary>
        ErrorOccured = 3
    }

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
