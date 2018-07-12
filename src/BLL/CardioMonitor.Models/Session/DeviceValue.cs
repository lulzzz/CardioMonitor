using System;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Статус показателя пациента, запрашиваемого с оборудования
    /// </summary>
    public enum DeviceValueStatus
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
        Obtained  =2,
        
        /// <summary>
        /// Произошла ошибка запроса показателя
        /// </summary>
        ErrorOccured = 3
    }
    
    /// <summary>
    /// Значение, полученное с внешнего устройства
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct DeviceValue<T>
    {
        public DeviceValueStatus Status { get; }
        
        /// <summary>
        /// Данные, полученные от устройства
        /// </summary>
        public T Value { get; }
        
        public DeviceValue(T value)
        {
            Value = value;
            Status = DeviceValueStatus.Obtained;
        }

        public DeviceValue(DeviceValueStatus status)
        {
            if (status == DeviceValueStatus.Obtained) 
                throw new ArgumentException($"Необходимо установить значения при выбранном статусе");

            Status = status;
            Value = default(T);
        }

        public DeviceValue(T value, DeviceValueStatus status)
        {
            Value = value;
            Status = status;
        }
        
        public override string ToString()
        {
            switch (Status)
            {
                case DeviceValueStatus.NotObtained:
                    return "-";
                case DeviceValueStatus.ErrorOccured:
                    return "ошибка";
                case DeviceValueStatus.Unknown:
                    return "неизвестно";
                default:
                    return Value.ToString();
            }

        }
    }
}