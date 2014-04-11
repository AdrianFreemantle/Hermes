using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

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
                    var result = ExecuteScalar<T>(database, command, parameters);
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

        private static T ExecuteScalar<T>(Database database, string command, IEnumerable<object> parameters)
        {
            DbCommand cmd = database.Connection.CreateCommand();
            cmd.CommandText = command;
            cmd.CommandType = CommandType.Text;
            
            if(parameters != null && parameters.Any())
                cmd.Parameters.Add(parameters);

            using (var result = cmd.ExecuteReader(CommandBehavior.SingleResult))
            {
                if (result.Read())
                {
                    return (T)result[0];
                }

                result.Close();
            }
            
            return default(T);
        }
    }
}