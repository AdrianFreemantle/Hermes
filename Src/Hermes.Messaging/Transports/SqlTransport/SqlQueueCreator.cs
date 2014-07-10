using System;
using System.Data.SqlClient;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Transports.SqlTransport
{
    public class SqlQueueCreator : ICreateMessageQueues
    {
        private readonly string connectionString;

        public SqlQueueCreator()
        {
            connectionString = Settings.GetSetting<string>(SqlTransportConfiguration.MessagingConnectionStringKey);
        }

        public void CreateQueueIfNecessary(Address address)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(String.Format(SqlCommands.CreateQueue, address), connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Purge(Address address)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(String.Format(SqlCommands.PurgeQueue, address), connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
 