namespace CardioMonitor.FileSaving.Containers
{
    /// <summary>
    /// Контейнер для хранения информации о сеанса в виде файла
    /// </summary>
    internal class FileStorageDataContainer
    {
        /// <summary>
        /// Версия данных
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Сериализованных в json данные
        /// </summary>
        public string DataJson { get; set; }
    }
}
