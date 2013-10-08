﻿using System;
using System.Data;
using System.Data.SqlClient;

namespace Hermes
{
    public class TransactionalSqlConnection : IDisposable
    {
        private readonly SqlConnection connection;
        private readonly SqlTransaction transaction;
        private bool disposed;

        private TransactionalSqlConnection(string connectionString, IsolationLevel isolationLevel)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            transaction = connection.BeginTransaction(isolationLevel);
        }

        public static TransactionalSqlConnection Begin(string connectionString)
        {
            return new TransactionalSqlConnection(connectionString, IsolationLevel.ReadCommitted);
        }

        public static TransactionalSqlConnection Begin(string connectionString, IsolationLevel isolationLevel)
        {
            return new TransactionalSqlConnection(connectionString, isolationLevel);
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Rollback()
        {
            transaction.Rollback();
        }

        public SqlCommand BuildCommand(string sql, params SqlParameter[] parameters)
        {
            return BuildCommand(sql, CommandType.Text, parameters);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public SqlCommand BuildCommand(string sql, CommandType commandType, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(sql, connection, transaction) {CommandType = commandType};

            foreach (var sqlParameter in parameters)
            {
                command.Parameters.Add(sqlParameter);
            }

            return command;
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
            }

            disposed = true;
        }
    }
}