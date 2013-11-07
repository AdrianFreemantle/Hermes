using System;

using Hermes.Messaging.Configuration;
using Hermes.Sql;

namespace Hermes.Messaging.Transports.SqlTransport
{
    public class SqlQueueCreator : ICreateQueues
    {
        private readonly string connectionString;

        public SqlQueueCreator()
        {
            connectionString = Settings.GetSetting<string>(SqlTransportConfiguration.MessagingConnectionStringKey);
        }

        public void CreateQueueIfNecessary(Address address)
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                var command = connection.BuildCommand(String.Format(SqlCommands.CreateQueue, address));
                command.ExecuteNonQuery();
                connection.Commit();
            }
        }

        public void Purge(Address address)
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                var command = connection.BuildCommand(String.Format(SqlCommands.PurgeQueue, address));
                command.ExecuteNonQuery();
                connection.Commit();
            }
        }
    }
}
 