using System;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Bed.Infrastructure
{
    /// <summary>
    /// Контроллер для взаимодействия с инверсионным столом
    /// </summary>
    public interface IBedController : IDeviceController
    {
        #region Управление взаимодействием и обработка событий
        
        /// <summary>
        /// Событие нажатия на кнопку `Пауза` на пульте управления инверсионным столом
        /// </summary>
        event EventHandler OnPauseFromDeviceRequested;
        
        /// <summary>
        /// Событие нажатия на кнопку `Продолжение` на пульте управления инверсионным столом
        /// </summary>
        event EventHandler OnResumeFromDeviceRequested;
        
        /// <summary>
        /// Событие нажатия на кнопку `Экстренная остановка` на пульте управления инверсионным столом
        /// </summary>
        event EventHandler OnEmeregencyStopFromDeviceRequested;

        /// <summary>
        /// События нажатия на кнопку `Реверс` на пульте управления инверсионным столом
        /// </summary>
        event EventHandler OnReverseFromDeviceRequested;
        
        /// <summary>
        /// Возвращает признак подключения к устройству
        /// </summary>
        /// <returns></returns>
        bool IsConnected { get; }

        /// <summary>
        /// Инициализирует контроллер
        /// </summary>
        /// <param name="initParams">Параметры инициализации</param>
        /// <remarks>
        /// Необходимо выполнять инициализацию перед всеми действиями
        /// </remarks>
        void Init(IBedControllerInitParams initParams);
        
        /// <summary>
        /// Устанавливает подключение к инверсионному столу
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Перед подключением необходмо проинициализировать контроллер с помощью метода <see cref="Init"/>
        /// </remarks>
        Task ConnectAsync();

        /// <summary>
        /// Отключает контроллер от инверсионного стола
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();
        
        /// <summary>
        /// Отправляет инверсионному столу команду для выполнения
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task ExecuteCommandAsync(BedControlCommand command);
        
        #endregion
        
        #region Получение данных
        
        /// <summary>
        /// Возвращает длительность одного цикла
        /// </summary>
        /// <returns></returns>
        Task<TimeSpan> GetCycleDurationAsync();

        /// <summary>
        /// Возвращает количество циклов в сеансе
        /// </summary>
        /// <returns></returns>
        Task<short> GetCyclesCountAsync();
        
        /// <summary>
        /// Возвращает номер текущего цикла в сеансе
        /// </summary>
        /// <returns></returns>
        Task<short> GetCurrentCycleNumberAsync();

        /// <summary>
        /// Возвращает количество итераций в цикле
        /// </summary>
        /// <returns></returns>
        Task<short> GetIterationsCountAsync();

        /// <summary>
        /// Возвращает номер текущей итерации
        /// </summary>
        /// <returns></returns>
        Task<short> GetCurrentIterationAsync();

        /// <summary>
        /// Возвращает номер ближайшей итерации для снятия параметров давления
        /// </summary>
        /// <returns></returns>
        Task<short> GetNextIterationNumberForPressureMeasuringAsync();
        
        /// <summary>
        /// Возвращает номер ближейшей итерации для снятия общих параметров пациента
        /// </summary>
        /// <returns></returns>
        Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync();

        /// <summary>
        /// Возвращает номер ближайшей итерации для снятия ЭКГ
        /// </summary>
        /// <returns></returns>
        Task<short> GetNextIterationNumberForEcgMeasuringAsync();

        /// <summary>
        /// Возвращает время, оставшеся до конца сеанса
        /// </summary>
        /// <returns></returns>
        Task<TimeSpan> GetRemainingTimeAsync();

        /// <summary>
        /// Возвращает время, прошедшее с начала сеанса
        /// </summary>
        /// <returns></returns>
        Task<TimeSpan> GetElapsedTimeAsync();
        
        /// <summary>
        /// Возвращает признак начала работы кровати
        /// </summary>
        /// <returns></returns>
        Task<StartFlag> GetStartFlagAsync();

        /// <summary>
        /// Возвращает флаг реверсного движения кровати
        /// </summary>
        /// <returns></returns>
        Task<ReverseFlag> GetReverseFlagAsync();
        
        /// <summary>
        /// Возвращает угол наклона кровати по оси Х
        /// </summary>
        /// <returns></returns>
        Task<float> GetAngleXAsync();
        
        #endregion
    }
}