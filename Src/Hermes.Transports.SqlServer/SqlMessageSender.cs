using System;
using System.Data;
using System.Data.SqlClient;

using Hermes.Configuration;
using Hermes.Messaging;
using Hermes.Serialization;

namespace Hermes.Transports.SqlServer
{
    public class SqlMessageSender : ISendMessages
    {
        private readonly ISerializeObjects objectSerializer;   

        private readonly string connectionString;

        public SqlMessageSender(ISerializeObjects objectSerializer)
        {
            this.objectSerializer = objectSerializer;
            connectionString = Settings.GetSetting<string>(SqlMessagingConfiguration.MessagingConnectionStringKey);
        }

        public void Send(MessageEnvelope message, Address address)
        {
            using (var transactionalConnection = TransactionalSqlConnection.Begin(connectionString))
            {
                using (var command = BuildSendCommand(transactionalConnection, message, address))
                {
                    command.ExecuteNonQuery();
                }

                transactionalConnection.Commit();
            }
        }

        private SqlCommand BuildSendCommand(TransactionalSqlConnection connection, MessageEnvelope message, Address address)
        {
            var command = connection.BuildCommand(String.Format(SqlCommands.Send, address.Queue));
            command.CommandType = CommandType.Text;

            command.Parameters.AddWithValue("@Id", message.MessageId);
            command.Parameters.AddWithValue("@Recoverable", message.Recoverable);
            command.Parameters.AddWithValue("@Headers", objectSerializer.SerializeObject(message.Headers));
            command.Parameters.AddWithValue("@Body", message.Body);

            if (message.CorrelationId != Guid.Empty)
            {
                command.Parameters.AddWithValue("@CorrelationId", message.CorrelationId);
            }
            else
            {
                command.Parameters.AddWithValue("@CorrelationId", DBNull.Value);
            }

            if (message.HasExpiryTime)
            {
                command.Parameters.AddWithValue("@Expires", message.ExpiryTime);
            }
            else
            {
                command.Parameters.AddWithValue("@Expires", DBNull.Value);
            }

            return command;
        }
    }
}
