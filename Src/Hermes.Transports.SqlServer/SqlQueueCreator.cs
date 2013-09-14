using System.Data;
using System.Data.SqlClient;

using Hermes.Configuration;
using Hermes.Messaging;

namespace Hermes.Transports.SqlServer
{
    public class SqlQueueCreator : ICreateQueues
    {
        private readonly string connectionString;

        public SqlQueueCreator()
        {
            connectionString = Settings.GetSetting<string>(SqlMessagingConfiguration.MessagingConnectionStringKey);
        }

        public void CreateQueueIfNecessary(Address address)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(string.Format(SqlCommands.CreateQueue, address.Queue), connection))
                {
                    command.CommandType = CommandType.Text; 
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
 