using System;
using System.Data;
using System.Data.SqlClient;
using Hermes.Failover;
using Hermes.Messaging.Configuration;
using Hermes.Serialization;

namespace Hermes.Messaging.Transports.SqlTransport
{
    public class SqlMessageSender : ISendMessages
    {
        private readonly ISerializeObjects objectSerializer;   

        private readonly string connectionString;

        public SqlMessageSender(ISerializeObjects objectSerializer)
        {
            this.objectSerializer = objectSerializer;
            connectionString = Settings.GetSetting<string>(SqlTransportConfiguration.MessagingConnectionStringKey);
        }

        public void Send(TransportMessage transportMessage, Address address)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = BuildSendCommand(connection, transportMessage, address))
                {
                    command.ExecuteNonQuery();
                }
            }
        }        

        private SqlCommand BuildSendCommand(SqlConnection connection, TransportMessage transportMessage, Address address)
        {
            var command = connection.CreateCommand();
            
            command.CommandText = String.Format(SqlCommands.Send, address);
            command.CommandType = CommandType.Text;

            command.Parameters.AddWithValue("@Id", transportMessage.MessageId);
            command.Parameters.AddWithValue("@ReplyTo", transportMessage.ReplyToAddress.ToString());
            command.Parameters.AddWithValue("@Headers", objectSerializer.SerializeObject(transportMessage.Headers));
            command.Parameters.AddWithValue("@Body", transportMessage.Body);

            if (transportMessage.CorrelationId != Guid.Empty)
            {
                command.Parameters.AddWithValue("@CorrelationId", transportMessage.CorrelationId);
            }
            else
            {
                command.Parameters.AddWithValue("@CorrelationId", DBNull.Value);
            }

            if (transportMessage.HasExpiryTime)
            {
                command.Parameters.AddWithValue("@Expires", transportMessage.ExpiryTime);
            }
            else
            {
                command.Parameters.AddWithValue("@Expires", DBNull.Value);
            }

            return command;
        }
    }
}
