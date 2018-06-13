namespace CardioMonitor.BLL.SessionProcessing
{
    /// <summary>
    /// Значение, полученное с внешнего устройства
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct DeviceValue<T>
    {
        /// <summary>
        /// Признак удачного получения данных с устройства
        /// </summary>
        public bool IsValueObtained { get; }
        
        /// <summary>
        /// Признак ошибки при получении данных с устройства
        /// </summary>
        public bool IsErrorOccured { get; }
        
        /// <summary>
        /// Данные, полученные от устройства
        /// </summary>
        public T Value { get; }

        
        public DeviceValue(T value)
        {
            Value = value;
            IsValueObtained = true;
            IsErrorOccured = false;
        }

        public DeviceValue(bool isErrorOccured)
        {
            IsValueObtained = false;
            IsErrorOccured = isErrorOccured;
            Value = default(T);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}