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
            Context = contextFactory.GetContext();
        }

        public void Commit()
        {
            Context.SaveChanges();           
            Context.Database.Connection.Close();
        }

        public void Rollback()
        {
            Context.Dispose();
            Context = contextFactory.GetContext();
        }

        public EntityFrameworkRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
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