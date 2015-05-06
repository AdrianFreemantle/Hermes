using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using Hermes.Logging;

namespace Hermes.EntityFramework
{
    public static class DatabaseExtensions
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (DatabaseExtensions));

        public static T ExecuteScalarCommand<T>(this Database database, string command, params object[] parameters)
        {
            try
            {
                database.Connection.Open();
                Logger.Debug("Executing scalar command {0}", command);
                var result = ExecuteScalar<T>(database, command, parameters);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception while executing scalar command {0} {1}", command, ex.GetFullExceptionMessage());
                throw;
            }
            finally
            {
                database.Connection.Close();
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