namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Тип события при обмене данными
    /// </summary>
    public enum BedMessageEventType
    {
        Read = 3, //запрос на чтение данных
        Write = 6, //запрос на запись данных
        ReadAll = 0x13,
        WriteAll = 0x16
    }
}
