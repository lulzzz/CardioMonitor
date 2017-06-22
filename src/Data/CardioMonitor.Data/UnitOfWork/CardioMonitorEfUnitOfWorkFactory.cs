using System;
using CardioMonitor.Data.Common.UnitOfWork;
using CardioMonitor.Data.Contracts.UnitOfWork;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Ef.UnitOfWork
{
    public class CardioMonitorEfUnitOfWorkFactory : ICardioMonitorUnitOfWorkFactory
    {
        private readonly string _connectionString;

        public CardioMonitorEfUnitOfWorkFactory([NotNull] string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
        }
        
        public ICardioMonitorUnitOfWork Create()
        {
            return new CardioMonitorEfUnitOfWork(new UnitOfWorkContext(new CardioMonitorContext(_connectionString)));
        }
    }
}