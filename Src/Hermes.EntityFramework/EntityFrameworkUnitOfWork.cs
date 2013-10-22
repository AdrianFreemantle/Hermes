using System;
using System.Data.Entity;

namespace Hermes.EntityFramework
{
    public class EntityFrameworkUnitOfWork : IEntityUnitOfWork
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

        public EntityFrameworkRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (Context != null)
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