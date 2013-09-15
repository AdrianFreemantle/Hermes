using System;
using System.Data.Entity;

namespace MyDomain.Infrastructure.EntityFramework
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork
    {
        private readonly IContextFactory contextFactory;
        private DbContext context;
        private bool isDisposed;

        public EntityFrameworkUnitOfWork(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
            context = contextFactory.GetContext();
        }

        public void Commit()
        {
            context.SaveChanges();
        }

        public void Rollback()
        {
            context.Dispose();
            context = contextFactory.GetContext();
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return new EntityFrameworkRepository<TEntity>(context.Set<TEntity>());
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            context.Dispose();
        }
    }
}