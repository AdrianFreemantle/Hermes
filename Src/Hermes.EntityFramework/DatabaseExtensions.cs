using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;

namespace Hermes.EntityFramework
{
    public static class DatabaseExtensions
    {
        public static T ExecuteScalarCommand<T>(this Database database, string command, params object[] parameters)
        {
            using (var dbContextTransaction = database.BeginTransaction())
            {
                try
                {
                    T result = ExecuteScalar<T>(database, command, parameters);
                    dbContextTransaction.Commit();
                    return result;
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }
        }

        private static T ExecuteScalar<T>(Database database, string command, object[] parameters)
        {
            DbCommand cmd = database.Connection.CreateCommand();
            cmd.CommandText = command;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(parameters);
            return (T)cmd.ExecuteScalar();
        }
    }
}