using System;
using CardioMonitor.Data.Common.UnitOfWork;
using CardioMonitor.Data.Contracts.UnitOfWork;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Ef.UnitOfWork
{
    public class CardioMonitorUnitOfWorkFactory : ICardioMonitorUnitOfWorkFactory
    {
        private readonly CardioMonitorContext _context;

        public CardioMonitorUnitOfWorkFactory([NotNull] CardioMonitorContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
        }
        
        public ICardioMonitorUnitOfWork Create()
        {
            return new CardioMonitorUnitOfWork(new UnitOfWorkContext(_context));
        }
    }
}