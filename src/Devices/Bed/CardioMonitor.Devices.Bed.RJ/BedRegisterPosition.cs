namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Описание номеров регистров, в которых содержатся те или иные данные
    /// </summary>
    public static class BedRegisterPosition
    {
        public static  byte BedStatusPosition = 15;
        public static byte CurrentCyclePosition = 37;
        public static byte CurrentIterationPosition = 39;
        public static byte RemainingTimePosition = 46;
        public static byte ElapsedTimePosition = 48;
        public static byte BedTargetAngleXPosition = 50;
    }
}