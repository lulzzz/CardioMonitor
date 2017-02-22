using System;
using System.Data.Entity;
using System.Threading.Tasks;
namespace CardioMonitor.Data.Common.UnitOfWork
{
    /// <summary>
    /// Абстрактная обертка над EntityFramework DbContext для поддержки паттерна Unit of Work
    /// </summary>
    public interface IUnitOfWorkContext : IDisposable
    {
        /// <summary>
        /// Возвращает EntityFramework DbContext
        /// </summary>
        /// <returns></returns>
        DbContext Context { get; }

        /// <summary>
        /// Сохраняет изменения, сделанные в контексте
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Асинхронно сохраняет изменения, сделанные в контексте
        /// </summary>
        Task SaveChangesAsync();

        void Commit();

        void Rollback();

        DbContextTransaction BeginTransaction();
    }
}