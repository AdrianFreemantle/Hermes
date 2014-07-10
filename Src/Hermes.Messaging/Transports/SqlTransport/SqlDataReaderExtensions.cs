using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Hermes.Serialization;

namespace Hermes.Messaging.Transports.SqlTransport
{
    public static class SqlDataReaderExtensions
    {
        const int MessageIdIndex = 0;
        const int CorrelationIdIndex = 1;
        const int ReplyToAddressIndex = 2;
        const int TimeToLiveIndex = 3;
        const int HeadersIndex = 4;
        const int BodyIndex = 5;

        public static TransportMessage BuildTransportMessage(this SqlDataReader dataReader, ISerializeObjects objectSerializer)
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