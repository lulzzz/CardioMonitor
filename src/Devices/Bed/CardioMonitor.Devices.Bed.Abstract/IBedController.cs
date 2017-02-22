using System.Threading.Tasks;

namespace CardioMonitor.Devices.Bed.Infrastructure
{
    public interface IBedController : IDeviceController
    {
        /// <summary>
        /// Запрос текущего состояния устройства
        /// </summary>
        BedStatus GetBedStatus();

        /// <summary>
        /// Запрос текущего статуса движения кровати устройства 
        /// </summary>
        BedMovingStatus GetBedMovingStatus();


        /// <summary>
        /// Запрос флага старт/пауза (0 - пауза, 1- старт, -1 - изначальное состояние) и флага реверса (0 - реверс не вызван, 1 - вызван, -1 - изначальное состояние)
        /// </summary>
        //todo Мне не нравится этот метод в принципе, надо от него будет избавиться
        void UpdateFlags();

        /// <summary>
        /// Возвращает признак начала работы кровати
        /// </summary>
        /// <returns></returns>
        StartFlag GetStartFlag();

        /// <summary>
        /// Возвращает флаг реверсного движения кровати
        /// </summary>
        /// <returns></returns>
        ReverseFlag GetReverseFlag();

        /// <summary>
        /// Возвращает угол наклона кровати по оси Х
        /// </summary>
        /// <returns></returns>
        Task<double> GetAngleXAsync();

        /// <summary>
        /// Возвращает угол наклона кровати по оси Y
        /// </summary>
        double GetAngleY();
        
        /// <summary>
        /// Возвращает признак подключения устройство
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        void ExecuteCommand(BedControlCommand command);
    }
}