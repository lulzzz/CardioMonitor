using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing
{
    public class CycleData
    {
        public short CycleNumber { get;  }

        public ICollection<CheckPointParams> CycleParams { get; }

        //todo Add ecg data here 

        public event EventHandler<CheckPointParams> CheckPointAdded;
        
        public CycleData(short cycleNumber)
        {
            CycleNumber = cycleNumber;
            CycleParams = new List<CheckPointParams>();
        }

        public void AddCheckPoint([NotNull] CheckPointParams checkPoint)
        {
            if (checkPoint == null) throw new ArgumentNullException(nameof(checkPoint));

            CycleParams.Add(checkPoint);
            CheckPointAdded?.Invoke(this, checkPoint);
        }

        public void AddCheckPoints([NotNull] ICollection<CheckPointParams> checkPoint)
        {
            if (checkPoint == null) throw new ArgumentNullException(nameof(checkPoint));

            if (checkPoint.Count == 0) return;

            foreach (var checkPointParams in checkPoint)
            {
                if (checkPointParams == null) continue;

                AddCheckPoint(checkPointParams);
            }
        }
    }
}