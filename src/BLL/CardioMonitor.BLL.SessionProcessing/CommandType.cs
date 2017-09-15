namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Тип команды
    /// </summary>
    /// <remarks>
    /// Чтобы потом при логировании понимать, кто и что вызвал
    /// </remarks>
    public enum CommandType
    {
        /// <summary>
        /// Команды пришла от UI
        /// </summary>
        FromUI,
        /// <summary>
        /// Команды пришла от устройства
        /// </summary>
        FromDevice
    }
}