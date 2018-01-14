using System;
using CardioMonitor.BLL.SessionProcessing.CheckPoints;

namespace CardioMonitor.BLL.SessionProcessing.Processing
{
    public class CycleDurationCalculator
    {
        private readonly CheckPointAnglesFactory _anglesFactory;
        private const double IterationStep = 1.5;
        
        public TimeSpan CalculateDuraation(SessionParams sessionParams)
        {
//            var checkPointAngles = _anglesFactory.Create(sessionParams.MaxAngle);
//
//            var maxAngle = checkPointAngles.Max();
//
//            var iterationCount = maxAngle / IterationStep;
//            
//            var     
            return TimeSpan.FromMinutes(20);
        }
    }
}