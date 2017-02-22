using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace CardioMonitor.Data.Common.UnitOfWork
{
    /// <summary>
    /// Интерфейс для сохранения изменений, сделанных для в <see cref="IUnitOfWorkContext"/>
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Сохраняет изменения, сделанные во всех контекстах <see cref="IUnitOfWorkContext"/>
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Асинхронно сохраняет изменения, сделанные во всех контекстах <see cref="IUnitOfWorkContext"/>
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Возвращает  DbContext
        /// </summary>
        /// <returns></returns>
        IUnitOfWorkContext Context { get; }

        void Commit();

        void Rollback();

        DbContextTransaction BeginTransaction();


    }
}