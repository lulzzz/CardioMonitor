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
            Context.SaveChanges();
        }

        public void Rollback()
        {
            Context.Rollback();
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