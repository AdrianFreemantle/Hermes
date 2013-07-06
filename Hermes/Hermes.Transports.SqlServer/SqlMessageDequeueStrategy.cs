using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Hermes.Serialization;

namespace Hermes.Transports.SqlServer
{
    public class SqlMessageDequeueStrategy : IMessageDequeueStrategy
    {
        private readonly string connectionString;
        private readonly ISerializeObjects objectSerializer;        

        private const string SqlReceive =
            @"WITH message AS (SELECT TOP(1) * FROM [@queue] WITH (UPDLOCK, READPAST, ROWLOCK) ORDER BY [RowVersion] ASC) 
            DELETE FROM message 
            OUTPUT deleted.Id, deleted.CorrelationId, deleted.ReplyToAddress, 
            deleted.Recoverable, deleted.Expires, deleted.Headers, deleted.Body;";

        public SqlMessageDequeueStrategy(string connectionString, ISerializeObjects objectSerializer)
        {
            this.connectionString = connectionString;
            this.objectSerializer = objectSerializer;
        }

        public void Dequeue(Address address, Func<MessageEnvelope, bool> processMessage)
        {
            using (var transactionalConnection = TransactionalSqlConnection.Begin(connectionString))
            using (var command = transactionalConnection.BuildCommand(SqlReceive, new SqlParameter("@queue", address.Queue)))
            {
                
                var message = FetchNextMessage(command);

                if (message == MessageEnvelope.Undefined)
                {
                    transactionalConnection.Commit();
                    return;
                }

                TryProcessMessage(processMessage, message);
                transactionalConnection.Commit();
            }
        }

        private static void TryProcessMessage(Func<MessageEnvelope, bool> processMessage, MessageEnvelope messageEnvelope)
        {
            try
            {
                if (!processMessage(messageEnvelope))
                {
                    throw new MessageProcessingFailedException(messageEnvelope);
                }
            }
            catch (Exception ex)
            {
                throw new MessageProcessingFailedException(messageEnvelope, ex);
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
                    var correlationId = dataReader.GetGuid(1);
                    var replyToAddress = dataReader.IsDBNull(2) ? null : Address.Parse(dataReader.GetString(2));
                    var recoverable = dataReader.GetBoolean(3);
                    var headers = objectSerializer.DeserializeObject<Dictionary<string, string>>(dataReader.GetString(5));
                    var body = dataReader.IsDBNull(6) ? null : dataReader.GetSqlBinary(6).Value;

                    return new MessageEnvelope(messageId, correlationId, replyToAddress, timeTolive, recoverable, headers, body);
                }
            }

            return MessageEnvelope.Undefined;
        }

        private static TimeSpan GetTimeTolive(SqlDataReader dataReader)
        {
            if (dataReader.IsDBNull(4))
            {
                return TimeSpan.MaxValue;
            }

            DateTime expireDateTime = dataReader.GetDateTime(4);

            if (dataReader.GetDateTime(4) < DateTime.UtcNow)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks(expireDateTime.Ticks - DateTime.UtcNow.Ticks);
        }
    }
}