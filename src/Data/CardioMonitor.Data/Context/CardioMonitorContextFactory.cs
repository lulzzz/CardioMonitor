using System;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Ef.Context
{
    public class CardioMonitorContextFactory : ICardioMonitorContextFactory
    {
        private readonly string _connectionString;

        public CardioMonitorContextFactory([NotNull] string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public CardioMonitorContext Create()
        {
            return new CardioMonitorContext(_connectionString);
        }
    }
}