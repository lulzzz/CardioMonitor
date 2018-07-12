using System;
using System.Data.Entity;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Common.UnitOfWork
{
    public abstract class UnitOfWork : IUnitOfWork
    {
        [NotNull]
        public IUnitOfWorkContext Context { get; }

        private DbContextTransaction _transaction;

        protected UnitOfWork(IUnitOfWorkContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Context = context;
        }


        /// <summary>
        /// <see cref="IUnitOfWork.SaveChanges" />
        /// </summary>
        public void SaveChanges()
        {
            Context.SaveChanges();
        }

        /// <summary>
        /// <see cref="IUnitOfWork.SaveChangesAsync" />
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public void Commit()
        {
            //todo надо ли это?
            Context.SaveChanges();
            _transaction?.Commit();
        }

        public void Rollback()
        {
            //todo надо ли это?
            Context.Rollback();

            _transaction?.Rollback();
        }

        public bool BeginTransation()
        {
            _transaction = BeginTransaction();
            return _transaction != null;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Context.BeginTransaction();
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (disposing)
            {
                _transaction?.Dispose();
                Context.Dispose();
            }

            disposedValue = true;
        }

        /// <summary>
        /// Disposes of the disposable resources of the UnitOfWork class instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}