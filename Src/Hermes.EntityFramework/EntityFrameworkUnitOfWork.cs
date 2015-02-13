using System;
using System.Data.Entity;
using Hermes.Attributes;
using Hermes.Logging;
using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    [UnitOfWorkCommitOrder(Order = Int32.MaxValue)]
    public class EntityFrameworkUnitOfWork : IUnitOfWork, IRepositoryFactory
    {
        internal protected static readonly ILog Logger = LogFactory.BuildLogger(typeof(EntityFrameworkUnitOfWork));

        private readonly IContextFactory contextFactory;
        protected DbContext Context;
        private bool disposed;
        private bool committed;

        public EntityFrameworkUnitOfWork(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory; 
        }

        public void Commit()
        {
            if (Context != null)
            {
                Context.SaveChanges();
                committed = true;
            }
        }

        public void Rollback()
        {
            if (committed)
            {
                throw new UnitOfWorkRollbackException("The entity framework unit of work has already been committed and can therefore not revert the saved changes.");
            }

            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new()
        {
            var context = GetDbContext();

            return new EntityFrameworkRepository<TEntity>(context.Set<TEntity>()); 
        }

        public DbContext GetDbContext()
        {
            return Context ?? (Context = contextFactory.GetContext(ContextConfiguration.Transactional));
        }

        public Database GetDatabase()
        {
            return GetDbContext().Database;
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
                Context = null;
            }

            disposed = true;
        }
    }
}