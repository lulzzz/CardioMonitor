using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing
{
    public class CycleData
    {
        public short CycleNumber { get;  }

        public ObservableCollection<CheckPointParams> CycleParams { get; }
        
        public CycleData(short cycleNumber)
        {
            CycleNumber = cycleNumber;
            CycleParams = new ObservableCollection<CheckPointParams>();
        }

        public void AddCheckPoint([NotNull] CheckPointParams checkPoint)
        {
            if (checkPoint == null) throw new ArgumentNullException(nameof(checkPoint));

            CycleParams.Add(checkPoint);
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