using System;
using System.Data;
using System.Data.Entity;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    [UnitOfWorkCommitOrder(Order = Int32.MaxValue)]
    public class EntityFrameworkUnitOfWork : IUnitOfWork, IRepositoryFactory
    {
        internal protected static readonly ILog Logger = LogFactory.BuildLogger(typeof(EntityFrameworkUnitOfWork));

        private readonly IContextFactory contextFactory;
        protected DbContext Context;
        protected DbContextTransaction Transaction;
        private bool disposed;

        public static bool EnableDebugTrace { get; set; }

        public EntityFrameworkUnitOfWork(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory; 
        }

        public void Commit()
        {
            if (Transaction != null)
            {
                Logger.Debug("Commiting DbContextTransaction as part of unit-of-work commit");
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

        public IDbSet<TEntity> GetRepository<TEntity>() where TEntity : class, new()
        {
            var context = GetDbContext();

            return context.Set<TEntity>();
        }

        public DbContext GetDbContext()
        {
            if (Context == null)
            {
                Context = contextFactory.GetContext();

                if (EnableDebugTrace)
                {
                    Context.Database.Log = s => Logger.Info(s);
                }
            }

            return Context;
        }

        public Database GetDatabase()
        {
            return GetDbContext().Database;
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            Logger.Debug("Starting DbContextTransaction with isolation level : {0}", isolationLevel);
            var database = GetDatabase();
            database.Connection.Open();
            Transaction = database.BeginTransaction(isolationLevel);
        }

        public void CommitTransation()
        {
            Logger.Debug("Commiting DbContextTransaction");

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