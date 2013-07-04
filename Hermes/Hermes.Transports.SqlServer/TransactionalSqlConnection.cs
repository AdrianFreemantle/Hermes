using System;
using System.Data.SqlClient;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Hermes.Transports.SqlServer
{
    public class TransactionalSqlConnection : IDisposable
    {
        private readonly SqlConnection connection;
        private readonly SqlTransaction transaction;
        //private readonly TransactionScope scope;
        private bool disposed;

        public TransactionalSqlConnection(string connectionString)
        {
            //scope = new TransactionScope(TransactionScopeOption.Required);
            connection = new SqlConnection(connectionString);
            connection.Open();
            //transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void Commit()
        {
            transaction.Commit();
            //scope.Complete();
        }

        public void Rollback()
        {
            transaction.Rollback();   
        }

        public SqlCommand BuildCommand(string sql)
        {
            return new SqlCommand(sql, connection, transaction);
        }

        ~TransactionalSqlConnection()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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

            if (disposing)
            {
                transaction.Dispose();
                connection.Dispose();
                //scope.Dispose();
            }

            disposed = true;
        }
    }
}