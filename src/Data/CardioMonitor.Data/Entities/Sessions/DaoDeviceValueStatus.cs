namespace CardioMonitor.Data.Ef.Entities.Sessions
{
    /// <summary>
    /// Статус показателя пациента, запрашиваемого с оборудования
    /// </summary>
    public enum DaoDeviceValueStatus
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
}