using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Определитель контрольных точек и принятия решения о запросе данных, накачке и т.д.
    /// </summary>
    /// <remarks>
    /// Новый цикл - нужно сбросить состояния
    /// </remarks>
    internal class CheckPointResolver
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
        private readonly double _resolutionToleranceAngle;

        private readonly double _minCheckPointAngle;

        /// <summary>
        /// Смещение, чтобы определение первой ключевой точки сработало
        /// </summary>
        private const double AngleOffset = 10;
        
        private double _previuosCheckPointAngle;
        
        private readonly object _passedCheckPointsAnglesLockObject = new object();

        public CheckPointResolver([NotNull] double[] checkPointAngles)
        {
            if (checkPointAngles == null) throw new ArgumentNullException(nameof(checkPointAngles));
            if (checkPointAngles.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(checkPointAngles));
            
            _checkPointsAngles = new List<double> (checkPointAngles);
            _previuosCheckPointAngle = _checkPointsAngles.Min() - AngleOffset;
            _resolutionToleranceAngle = checkPointAngles.Max() / checkPointAngles.Length;
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
                        if (Math.Abs(_previuosCheckPointAngle - checkPointAngle) < _resolutionToleranceAngle) { return false; }

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
            _previuosCheckPointAngle =_checkPointsAngles.Max();
        }
    }
}