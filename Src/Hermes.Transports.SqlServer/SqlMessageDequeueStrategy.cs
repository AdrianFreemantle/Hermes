﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Hermes.Configuration;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Serialization;

namespace Hermes.Transports.SqlServer
{
    public class SqlMessageDequeueStrategy : IMessageDequeueStrategy
    {
        private readonly string connectionString;
        private readonly ISerializeObjects objectSerializer;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (SqlMessageDequeueStrategy));

        const int messageIdIndex = 0;
        const int correlationIdIndex = 1;
        const int replyToAddressIndex = 2;
        const int timeToLiveIndex = 3;
        const int headersIndex = 4;
        const int bodyIndex = 5;

        public SqlMessageDequeueStrategy(ISerializeObjects objectSerializer)
        {
            connectionString = Settings.GetSetting<string>(SqlMessagingConfiguration.MessagingConnectionStringKey);
            this.objectSerializer = objectSerializer;
        }

        public TransportMessage Dequeue(Address address)
        {
            try
            {
                return TryDequeue(address);
            }
            catch (Exception ex)
            {
                Logger.Fatal("Error while attempting to dequeue message: {0}", ex.GetFullExceptionMessage());
                return TransportMessage.Undefined;
            }
        }

        private TransportMessage TryDequeue(Address address)
        {
            using (var transactionalConnection = TransactionalSqlConnection.Begin(connectionString, IsolationLevel.ReadCommitted))
            {
                using (var command = transactionalConnection.BuildCommand(String.Format(SqlCommands.Dequeue, address.Queue)))
                {
                    var message = FetchNextMessage(command);
                    transactionalConnection.Commit();
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

                    var messageId = dataReader.GetGuid(messageIdIndex);
                    var correlationId = dataReader.IsDBNull(correlationIdIndex) ? Guid.Empty : Guid.Parse(dataReader.GetString(correlationIdIndex));
                    var replyToAddress = dataReader.GetString(replyToAddressIndex);
                    var headers = objectSerializer.DeserializeObject<Dictionary<string, string>>(dataReader.GetString(headersIndex));
                    var body = dataReader.IsDBNull(bodyIndex) ? null : dataReader.GetSqlBinary(bodyIndex).Value;

                    return new TransportMessage(messageId, correlationId, Address.Parse(replyToAddress), timeTolive, headers, body);
                }
            }

            return TransportMessage.Undefined;
        }

        private static TimeSpan GetTimeTolive(SqlDataReader dataReader)
        {
            if (dataReader.IsDBNull(timeToLiveIndex))
            {
                return TimeSpan.MaxValue;
            }

            DateTime expireDateTime = dataReader.GetDateTime(timeToLiveIndex);

            if (dataReader.GetDateTime(timeToLiveIndex) < DateTime.UtcNow)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks(expireDateTime.Ticks - DateTime.UtcNow.Ticks);
        }
    }
}