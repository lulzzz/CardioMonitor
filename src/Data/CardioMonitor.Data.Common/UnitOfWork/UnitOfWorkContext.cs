using System;
using System.Data.Entity;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Common.UnitOfWork
{
    public class UnitOfWorkContext : IUnitOfWorkContext
    {
        [NotNull]
        public DbContext Context { get; }

        public UnitOfWorkContext([NotNull] DbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        public void SaveChanges()
        {
            Context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public void Commit()
        {
            // Так ли?
            Context.SaveChanges();
        }

        public void Rollback()
        {
            // не знаю как пока,
            //todo узнать
        }

        public DbContextTransaction BeginTransaction()
        {
            return Context.Database.BeginTransaction();
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Context.Dispose();
                }

                disposedValue = true;
            }
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