using System;
using System.Data.Entity;

namespace Hermes.EntityFramework
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork, ILookupTableRepository
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

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return new EntityFrameworkRepository<TEntity>(Context.Set<TEntity>());
        }

        public TLookup Get<TLookup>(Enum id) where TLookup : ILookupTable
        {
            return Get<TLookup>((int)(dynamic)id);
        }

        public TLookup Get<TLookup>(int id) where TLookup : ILookupTable
        {
            return Context.Fetch<TLookup>(id);
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