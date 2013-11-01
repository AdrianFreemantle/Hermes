using System;
using System.Data.Entity;

using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork, IRepositoryFactory
    {
        private readonly IContextFactory contextFactory;
        protected DbContext Context;
        private bool disposed;

        public EntityFrameworkUnitOfWork(IContextFactory contextFactory)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Starting new EntityFrameworkUnitOfWork {0}", GetHashCode()));
            this.contextFactory = contextFactory;            
        }

        public void Commit()
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Committing EntityFrameworkUnitOfWork {0}", GetHashCode()));

            if (Context != null)
            {
                Context.SaveChanges();
                Context.Database.Connection.Close();
            }
        }

        public void Rollback()
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Rolling back EntityFrameworkUnitOfWork {0}", GetHashCode()));

            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }

        public EntityFrameworkRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (Context == null)
            {
                Context = contextFactory.GetContext();
            }

            return new EntityFrameworkRepository<TEntity>(Context);
        }

        ~EntityFrameworkUnitOfWork()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            System.Diagnostics.Trace.WriteLine(String.Format("Disposing EntityFrameworkUnitOfWork {0}", GetHashCode()));

            if (disposing && Context != null)
            {
                Context.Dispose();
            }

            disposed = true;
        }
    }
}