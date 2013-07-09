using System;
using System.Data;
using System.Data.SqlClient;

using Hermes.Configuration;
using Hermes.Serialization;

namespace Hermes.Transports.SqlServer
{
    public class SqlMessageSender : ISendMessages
    {
        private const string SqlSend = @"INSERT INTO [{0}] ([Id],[CorrelationId],[ReplyToAddress],[Recoverable],[Expires],[Headers],[Body]) 
                                         VALUES (@Id,@CorrelationId,@ReplyToAddress,@Recoverable,@Expires,@Headers,@Body)";

        private readonly ISerializeObjects objectSerializer;
        private readonly string connectionString;

        public SqlMessageSender(ISerializeObjects objectSerializer)
        {
            this.objectSerializer = objectSerializer;
            connectionString = Settings.GetSetting<string>(SqlMessagingSettings.MessagingConnectionStringKey);
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
            var command = connection.BuildCommand(String.Format(SqlSend, address.Queue));
            command.CommandType = CommandType.Text;

            command.Parameters.AddWithValue("@Id", message.MessageId);
            command.Parameters.AddWithValue("@ReplyToAddress", message.ReplyToAddress.ToString());
            command.Parameters.AddWithValue("@Recoverable", message.Recoverable);
            command.Parameters.AddWithValue("@Headers", objectSerializer.SerializeObject(message.Headers));
            command.Parameters.AddWithValue("Body", message.Body);

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
                command.Parameters.AddWithValue("Expires", message.ExpiryTime);
            }
            else
            {
                command.Parameters.AddWithValue("Expires", DBNull.Value);
            }

            return command;
        }
    }
}
