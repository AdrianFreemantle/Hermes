using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Hermes.Configuration;
using Hermes.Messaging;
using Hermes.Serialization;

namespace Hermes.Transports.SqlServer
{
    public class SqlMessageDequeueStrategy : IMessageDequeueStrategy
    {
        private readonly string connectionString;
        private readonly ISerializeObjects objectSerializer;

        public SqlMessageDequeueStrategy(ISerializeObjects objectSerializer)
        {
            connectionString = Settings.GetSetting<string>(SqlMessagingConfiguration.MessagingConnectionStringKey);
            this.objectSerializer = objectSerializer;
        }

        public MessageEnvelope Dequeue(Address address)
        {
            using (var transactionalConnection = TransactionalSqlConnection.Begin(connectionString))
            using (var command = transactionalConnection.BuildCommand(String.Format(SqlCommands.Dequeue, address.Queue)))
            {
                var message = FetchNextMessage(command);
                transactionalConnection.Commit();
                return message;
            }
        }

        MessageEnvelope FetchNextMessage(SqlCommand command)
        {
            using (var dataReader = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (dataReader.Read())
                {
                    var timeTolive = GetTimeTolive(dataReader);

                    if (timeTolive == TimeSpan.Zero)
                    {
                        return MessageEnvelope.Undefined;
                    }

                    var messageId = dataReader.GetGuid(0);
                    var correlationId = dataReader.IsDBNull(1) ? Guid.Empty : Guid.Parse(dataReader.GetString(1));
                    var recoverable = dataReader.GetBoolean(2);
                    var headers = objectSerializer.DeserializeObject<Dictionary<string, string>>(dataReader.GetString(4));
                    var body = dataReader.IsDBNull(5) ? null : dataReader.GetSqlBinary(5).Value;

                    return new MessageEnvelope(messageId, correlationId, timeTolive, recoverable, headers, body);
                }
            }

            return MessageEnvelope.Undefined;
        }

        private static TimeSpan GetTimeTolive(SqlDataReader dataReader)
        {
            if (dataReader.IsDBNull(3))
            {
                return TimeSpan.MaxValue;
            }

            DateTime expireDateTime = dataReader.GetDateTime(3);

            if (dataReader.GetDateTime(3) < DateTime.UtcNow)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks(expireDateTime.Ticks - DateTime.UtcNow.Ticks);
        }
    }
}