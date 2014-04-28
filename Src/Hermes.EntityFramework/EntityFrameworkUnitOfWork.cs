using System;
using System.Data;
using System.Data.Entity;

using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork, IRepositoryFactory
    {
        private readonly IContextFactory contextFactory;
        protected DbContext Context;
        protected DbContextTransaction Transaction;
        private bool disposed;

        public EntityFrameworkUnitOfWork(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory; 
        }

        public void Commit()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
            }

            if (Context != null)
            {
                Context.SaveChanges();
                Context.Database.Connection.Close();
            }
        }

        public void Rollback()
        {
            if (Transaction != null && Transaction.UnderlyingTransaction.Connection != null)
            {
                Transaction.Rollback();
            }

            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }

        public IQueryableRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new()
        {
            GetDbContext();

            return new EntityFrameworkRepository<TEntity>(Context);
        }

        protected DbContext GetDbContext()
        {
            return Context ?? (Context = contextFactory.GetContext());
        }

        public Database GetDatabase()
        {
            return GetDbContext().Database;
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            var database = GetDatabase();
            database.Connection.Open();
            Transaction = database.BeginTransaction(isolationLevel);
        }

        public void CommitTransation()
        {
            if (Transaction == null)
            {
                throw new NullReferenceException("A has not been started and can therefore not be committed");
            }

            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
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

            if (disposing && Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            disposed = true;
        }
    }
}