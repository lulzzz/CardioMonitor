using System;

namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    public class PatientParamsIterationNumber
    {

        public static int[] GetPatientParamsIterationNumberList(double maxAngle)
        {
            maxAngle = Math.Round(maxAngle, 1);
            if (maxAngle < 7.5 || maxAngle > 31.5) throw new ArgumentException("Заданный максимальный угол не подходит под условие");

            if (maxAngle < 13.5) return ECGIterationNumber.GetECGIterationNumberList(maxAngle); //для углов меньше 13,5 точки для экг и данных совпадают
            switch (maxAngle)
            {
                case 13.5:
                {
                    return new[] {0, 3, 8, 11, 15};
                }
                case 15.0:
                {
                    return new[] {0, 4, 9, 12, 17}; 
                }
                case 16.5:
                {
                    return new[] {0, 4, 10, 14, 19};
                }
                case 18.0:
                {
                    return new[] {0, 5, 11, 15, 21};
                }
                case 19.5:
                {
                    return new[] {0, 5, 12, 17, 23};
                }
                case 21.0:
                {
                    return new[] {0, 6, 13, 18, 25};
                }
                case 22.5:
                {
                    return new[] {0, 4, 9, 14, 17, 22, 27};
                }
                case 24.0:
                {
                    return new[] {0, 4, 9, 15, 19, 24, 29};
                }
                case 25.5:
                {
                    return new[] {0, 5, 10, 16, 20, 25, 31};
                }
                case 27.0:
                {
                    return new[] {0, 5, 11, 17, 21, 27, 33};
                }
                case 28.5:
                {
                    return new[] {0, 5, 12, 18, 22, 29, 35};
                }
                case 30.0:
                {
                    return new[] {0, 6, 12, 19, 24, 30, 37};
                }
                case 31.5:
                {
                    return new[] {0, 6, 13, 20, 25, 32, 39};
                }
            }
            throw new ArgumentException("Неизвестное значение угла");
        }
    }
}