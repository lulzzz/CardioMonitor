using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Значение регистров кровати
    /// </summary>
    /// <remarks>
    /// В классе присутствует только необходимые для приложения поля регистры, а не все возможные
    /// </remarks>
   public class BedRegisterValues
    {
        /// <summary>
        /// Уникальный номер кровати
        /// </summary>
        //todo а нужно ли нам это сейчас? - конкретно сейчас нет, но может пригодится чуть позже 
        public int BedID { get; set; }

        
        /// <summary>
        /// Статус сеанса. 0x0001 - подготовка к сеансу; 0x0000 - готов к запуску сеанса;
        ///  0x0002 - работа (сеанс запущен); 0x0005 -  неисправность (ошибка); 0x0003 - блокировка при старте с ПК
        /// </summary>
        public BedStatus BedStatus { get; set; }

        /// <summary> 
        /// Код ошибки
        /// </summary>
        /// todo напомнить уточнить необходимость отображения и списка ошибок
        public int BedError { get; set; }

       
        /// <summary>
        /// Блокирование кровати. Если 1, то старт сеанса происходит с ПК. Перед началом движения,
        ///  во время проверки давления неообходимо заблокировать кровать. Для исключения дублирующего запуска с кнопок.
        ///  Статус может снять только ПК или по таймауту через 5 мин
        /// режим стоит WO (только на запись?)
        /// </summary> 
        public bool IsBedBlocked { get; set; } //todo важная херня, не забыть про нее

        /// <summary>
        /// Частота для алгоритма движения, 1/sec
        /// </summary>
        public float Frequency { get; set; } //16 bit float

        /// <summary>
        /// Максимальный угол наклона
        /// </summary>
        public float MaxAngle { get; set; }  //16 bit float

        /// <summary>
        /// Количество циклов (повторений) в одном сеансе
        /// </summary>
        public short CycleCount { get; set; }

        /// <summary>
        /// Текущий цикл в сессии
        /// </summary>
        public short CurrentCycle { get; set; }

        /// <summary>
        /// Текущая итерация в цикле
        /// </summary>
        public short CurrentIteration { get; set; }

        /// <summary>
        /// Текущий шаг в итерации алгоритма(от 1 до 6)
        /// </summary>
        public short CurrentStep { get; set; }

        /// <summary>
        /// Следующая итерация в которой требуется измерить давление, нужно рассчитать
        /// </summary>
        //todo если рассчитываем, то имеет ли смысл хранить его здесь? Если не нужны, надо вынести в другое место
        //todo если будет убрано из возвращаемых значений с кровати - уберу
        public short NextIterationForPressureMeasuring { get; set; }

        /// <summary>
        /// Следующая итерация, в которой требуется измерить ЭКГ
        /// </summary>
        //todo если рассчитываем, то имеет ли смысл хранить его здесь? Если не нужны, надо вынести в другое место
        public short NextIterationForECG { get; set; }
        //todo NextIteration типа определены, но на деле не используются, ибо эти значения вычисляются на ПК

        /// <summary>
        /// Время до окончания сеанса
        /// </summary>
        public TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// Время от начала сеанса
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// Текущий желаемый угол по Х, для вывода на экран и в ПК
        /// </summary>
        public float BedTargetAngleX { get; set; }

        /// <summary>
        /// Текущий желаемый угол по Y, для вывода на экран и в ПК
        /// </summary>
        //todo не факт что будет использоваться за пределами сервисной программы
        public float BedTargetAngleY { get; set; }      
    }
}
