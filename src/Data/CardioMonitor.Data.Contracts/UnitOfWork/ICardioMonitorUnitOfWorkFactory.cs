using JetBrains.Annotations;

namespace CardioMonitor.Data.Contracts.UnitOfWork
{
    public interface ICardioMonitorUnitOfWorkFactory
    {
        [NotNull]
        ICardioMonitorUnitOfWork Create();
    }
}