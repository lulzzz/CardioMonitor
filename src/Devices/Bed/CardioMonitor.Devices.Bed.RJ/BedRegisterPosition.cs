namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Описание номеров регистров, в которых содержатся те или иные данные
    /// </summary>
    public static class BedRegisterPosition
    {
        public static  byte BedStatusPosition = 59;
        public static byte CurrentCyclePosition = 49;
        public static byte CurrentIterationPosition = 51;
        public static byte RemainingTimePosition = 54;
        public static byte ElapsedTimePosition = 56;
        public static byte BedTargetAngleXPosition = 186;

        /// <summary>
        /// Регистр блокировки
        /// <remarks> Перед началом работы отправляем 1 - кровать блокируется на вермя измерения давления
        /// после завершения, чтобы начать движение - сбрасываем до 0  </remarks>
        /// </summary>
        public static byte BedBlockPosition = 7;

        /// <summary>
        /// Регистр для установки частоты кровати
        /// </summary>
        public static byte BedFreqPosition = 20;

        
        /// <summary>
        /// Регистр для установки максимального угла подъема
        /// </summary>
        public static byte BedMaxAnglePosition = 21;

        /// <summary>
        /// Регистр для установки количества повторений
        /// </summary>
        public static byte BedCycleCountPosition = 22;
        
        /// <summary>
        /// номер регистра в который пишем команды для действий над кроватью
        /// </summary>
        public static byte BedMovingPosition = 56; 
        
        
    }
}