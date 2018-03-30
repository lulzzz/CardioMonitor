using System;

namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    public class ECGIterationNumber
    {

        public static int[] GetECGIterationNumberList(double maxAngle)
        {
            maxAngle = Math.Round(maxAngle, 1);
            if (maxAngle < 7.5 || maxAngle > 31.5) throw new ArgumentException("Заданный максимальный угол не подходит под условие");

            int iterationCount = (int)Math.Round((maxAngle / 1.5) * 2 - 3);
            int middlePoint = iterationCount / 2 + 1;
            return new []{0, middlePoint,iterationCount};

        }
        
    }
}