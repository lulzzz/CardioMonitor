using System;
using CardioMonitor.Data.Common.UnitOfWork;
using CardioMonitor.Data.Contracts.UnitOfWork;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Ef.UnitOfWork
{
    public class CardioMonitorEfUnitOfWorkFactory : ICardioMonitorUnitOfWorkFactory
    {
        
        public ICardioMonitorUnitOfWork Create()
        {
            return new CardioMonitorEfUnitOfWork(new UnitOfWorkContext(new CardioMonitorContext()));
        }
    }
}