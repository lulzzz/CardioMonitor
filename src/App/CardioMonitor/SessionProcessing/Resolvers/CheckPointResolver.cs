using System;
using System.Collections.Generic;

namespace CardioMonitor.SessionProcessing.Resolvers
{
    /// <summary>
    /// Определитель контрольных точек и принятия решения о запросе данных, накачке и т.д.
    /// </summary>
    /// <remarks>
    /// Новый цикл - новый инстанс
    /// </remarks>
    public class CheckPointResolver
    {

        /// <summary>
        /// Точность для сравнение double величин
        /// </summary>
        private const double Tolerance = 0.1e-12;

        /// <summary>
        /// Контрольные точки
        /// </summary>
        private readonly IReadOnlyList<double> _checkPointsAngles;

        /// <summary>
        /// Специальная велична, необходимая для корректной работы определения разрешения
        /// </summary>
        /// <remarks>
        /// Без нее все плохо, слишком часто вызывается метод
        /// </remarks>
        private const double ResolutionToleranceAgnle = 6;

        private double _previuosCheckPointAngle;
        


        private readonly object _passedCheckPointsAnglesLockObject = new object();

        public CheckPointResolver()
        {
            _checkPointsAngles = new List<double> { 0, 10.5, 21, 30 };
            _previuosCheckPointAngle = -10;
        }
        

        /// <summary>
        /// Проверяет, есть ли необходимость запросить данные в данном угле
        /// </summary>
        /// <param name="currentAngle">Текущий угол</param>
        /// <returns>Разрешение обновления</returns>
        public bool IsNeedUpdateData(double currentAngle)
        {
            lock (_passedCheckPointsAnglesLockObject)
            {
                if (_checkPointsAngles == null) { return false; }

                foreach (var checkPointAngle in _checkPointsAngles)
                {
                    if (Math.Abs(currentAngle - checkPointAngle) < Tolerance)
                    {
                        if (Math.Abs(_previuosCheckPointAngle - checkPointAngle) < ResolutionToleranceAgnle) { return false; }

                        _previuosCheckPointAngle = checkPointAngle;
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Учесть реверс
        /// </summary>
        public void ConsiderReversing()
        {
            _previuosCheckPointAngle = 30;
        }
    }
}