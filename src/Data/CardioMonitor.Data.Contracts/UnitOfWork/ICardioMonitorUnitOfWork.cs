using System.Security.Cryptography.X509Certificates;
using CardioMonitor.Data.Common.UnitOfWork;
using CardioMonitor.Data.Contracts.Repositories;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Contracts.UnitOfWork
{
    public interface ICardioMonitorUnitOfWork : IUnitOfWork
    {
        [NotNull]
        IPatientsRepository Patients { get; }

        [NotNull]
        ISessionsRepository Sessions { get; }

        [NotNull]
        ITreatmentsRepository Treatments { get; }
    }
}