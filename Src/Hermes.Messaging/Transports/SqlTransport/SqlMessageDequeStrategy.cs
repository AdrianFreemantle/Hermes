using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Serialization;

namespace Hermes.Messaging.Transports.SqlTransport
{
    public class SqlMessageDequeStrategy : IDequeueMessages
    {
        private readonly string connectionString;
        private readonly ISerializeObjects objectSerializer;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (SqlMessageDequeStrategy));
        private static readonly string DequeueSql = String.Format(SqlCommands.Dequeue, Address.Local);

        const int MessageIdIndex = 0;
        const int CorrelationIdIndex = 1;
        const int ReplyToAddressIndex = 2;
        const int TimeToLiveIndex = 3;
        const int HeadersIndex = 4;
        const int BodyIndex = 5;

        public SqlMessageDequeStrategy(ISerializeObjects objectSerializer)
        {
            connectionString = Settings.GetSetting<string>(SqlTransportConfiguration.MessagingConnectionStringKey);
            this.objectSerializer = objectSerializer;
        }

        public TransportMessage Dequeue()
        {
            try
            {
                return TryDequeue();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while attempting to dequeue message: {0}", ex.GetFullExceptionMessage());
                return TransportMessage.Undefined;
            }
        }

        private TransportMessage TryDequeue()
        {

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using(var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                using (var command = new SqlCommand(DequeueSql, connection, transaction))
                {
                    var message = FetchNextMessage(command);
                    transaction.Commit();
                    return message;
                }
            }
        }

        TransportMessage FetchNextMessage(SqlCommand command)
        {
            using (var dataReader = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (dataReader.Read())
                {
                    var timeTolive = GetTimeTolive(dataReader);

                    if (timeTolive == TimeSpan.Zero)
                    {
                        return TransportMessage.Undefined;
                    }

                    var messageId = dataReader.GetGuid(MessageIdIndex);
                    var correlationId = dataReader.IsDBNull(CorrelationIdIndex) ? Guid.Empty : Guid.Parse(dataReader.GetString(CorrelationIdIndex));
                    var replyToAddress = dataReader.GetString(ReplyToAddressIndex);
                    var headers = objectSerializer.DeserializeObject<Dictionary<string, string>>(dataReader.GetString(HeadersIndex));
                    var body = dataReader.IsDBNull(BodyIndex) ? null : dataReader.GetSqlBinary(BodyIndex).Value;

                    return new TransportMessage(messageId, correlationId, Address.Parse(replyToAddress), timeTolive, headers, body);
                }
            }

            return TransportMessage.Undefined;
        }

        private static TimeSpan GetTimeTolive(SqlDataReader dataReader)
        {
            if (dataReader.IsDBNull(TimeToLiveIndex))
            {
                return TimeSpan.MaxValue;
            }

            DateTime expireDateTime = dataReader.GetDateTime(TimeToLiveIndex);

            if (dataReader.GetDateTime(TimeToLiveIndex) < DateTime.UtcNow)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks(expireDateTime.Ticks - DateTime.UtcNow.Ticks);
        }
    }
}