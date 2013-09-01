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

        public void Send(TransportMessage transportMessage, Address address)
        {
            using (var transactionalConnection = TransactionalSqlConnection.Begin(connectionString))
            {
                using (var command = BuildSendCommand(transactionalConnection, transportMessage, address))
                {
                    command.ExecuteNonQuery();
                }

                transactionalConnection.Commit();
            }
        }

        private SqlCommand BuildSendCommand(TransactionalSqlConnection connection, TransportMessage transportMessage, Address address)
        {
            var command = connection.BuildCommand(String.Format(SqlCommands.Send, address.Queue));
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
