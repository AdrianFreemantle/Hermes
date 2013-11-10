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
            this.contextFactory = contextFactory;            
        }

        public void Commit()
        {
            if (Context != null)
            {
                Context.SaveChanges();
                Context.Database.Connection.Close();
            }
        }

        public void Rollback()
        {
            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new()
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

            if (disposing && Context != null)
            {
                Context.Dispose();
            }

            disposed = true;
        }
    }
}