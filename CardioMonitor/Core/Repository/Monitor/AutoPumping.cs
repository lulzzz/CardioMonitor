using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.Logs;
using CardioMonitor.ViewModel;

namespace CardioMonitor.Core.Repository.Monitor
{
    /// <summary>
    /// Отправка запроса на накачку давления
    /// </summary>
    public class AutoPumping
    {
        private double _previuosPumpingAngle;
        
        /// <summary> 
        /// Угол,используемый для задания диапазона значений текущего угла, для которых
        /// необходимо вызвать накачку
        /// </summary>
        private const double PumpingStartResolutionAgnle = 0.5;

        /// <summary>
        /// Специальная велична, необходимая для корректной работы определения разрешения
        /// </summary>
        /// <remarks>
        /// Без нее все плохо, слишком часто вызывается метод
        /// </remarks>
        private const double ResolutionToleranceAgnle = 6;

        /// <summary>
        /// Выполняет автонакачку
        /// </summary>
        /// <returns></returns>
        public Task<bool> Pump()
        {
            return Task.Factory.StartNew(() => AutoPumpingRequest.StartAutoPumpingRequest());
        }

        /// <summary>
        /// Проверяет, необходимо ли выполнить автонакачку по текущему углу
        /// </summary>
        /// <param name="currentAngle">Текущий угол</param>
        /// <param name="isUpping"></param>
        /// <returns></returns>
        public bool CheckNeedPumping(double currentAngle, bool isUpping)
        {
           
            // У нас на подъем/спуск на 30 градусов нужно 10 минут, 
            // т.е. каждую минуту движемся на 3 градуса. Значит,учитывая, что
            // автонакачка занимает примерно минуту, то за 3 градуса до контрольной точки
            // нужно подать разрешение о накачке
            if (/*(!isUpping) && */(currentAngle >= 30))             // без таой поправки угол предыдущей накачки при спуске не дает вызывать накачку к 21 и 10,5 градусам
            {
                _previuosPumpingAngle = 30;
            }
            // Для подъема
            if (isUpping && ((currentAngle >= 10.5 - PumpingStartResolutionAgnle && currentAngle <= 10.5)
                || (currentAngle >= 21 - PumpingStartResolutionAgnle && currentAngle <= 21)
                || (currentAngle >= 30 - PumpingStartResolutionAgnle && currentAngle <= 30)))
            {
                //Чтобы метод не вызывался слишком часто
                if (Math.Abs(currentAngle - _previuosPumpingAngle) < ResolutionToleranceAgnle)
                {
                    Logger.Instance.Log(String.Format("Current angle: {0}\t" +
                                              "PreviousAngle: {1}\t" +
                                              "Upping status: {2}\t" +
                                              "Result:        {3}\t", currentAngle, _previuosPumpingAngle, isUpping, false));
                    return false;
                }
                _previuosPumpingAngle = currentAngle;
                Logger.Instance.Log(String.Format("Current angle: {0}\t" +
                                              "PreviousAngle: {1}\t" +
                                              "Upping status: {2}\t" +
                                              "Result:        {3}\t", currentAngle, _previuosPumpingAngle, isUpping, true));
                return true;
            }
            // Для спуска
            if (!isUpping && ((currentAngle <= 10.5 + PumpingStartResolutionAgnle && currentAngle >= 10.5)
                || (currentAngle <= 21 + PumpingStartResolutionAgnle && currentAngle >= 21)
                || (currentAngle <= 0 + PumpingStartResolutionAgnle && currentAngle >= 0)))
            {
                //Чтобы метод не вызывался слишком часто
                if (Math.Abs(currentAngle - _previuosPumpingAngle) < ResolutionToleranceAgnle)
                {
                    Logger.Instance.Log(String.Format("Current angle: {0}\t" +
                                              "PreviousAngle: {1}\t" +
                                              "Upping status: {2}\t" +
                                              "Result:        {3}\t", currentAngle, _previuosPumpingAngle, isUpping, false));
                    return false;
                }
                _previuosPumpingAngle = currentAngle;
                Logger.Instance.Log(String.Format("Current angle: {0}\t" +
                                              "PreviousAngle: {1}\t" +
                                              "Upping status: {2}\t" +
                                              "Result:        {3}\t", currentAngle, _previuosPumpingAngle, isUpping, true));
                return true;
            }
            //Для всех остальных случаев
            Logger.Instance.Log(String.Format("Current angle: {0}\t" +
                                              "PreviousAngle: {1}\t" +
                                              "Upping status: {2}\t" +
                                              "Result:        {3}\t", currentAngle, _previuosPumpingAngle, isUpping, false));
            return false;
        }
    }
}
