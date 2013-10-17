using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Storage;
using Hermes.Serialization;

namespace Hermes.Storage.SqlServer
{
    public class SqlTimeoutStorage : IPersistTimeouts
    {       
        private readonly ITransportMessageFactory transportMessageFactory;
        private readonly ISerializeObjects objectSerializer;
        private readonly string connectionString;

        const int messageIdIndex = 0;
        const int correlationIdIndex = 1;
        const int destinationIndex = 2;
        const int timeToLiveIndex = 3;
        const int headersIndex = 4;
        const int bodyIndex = 5;

        public SqlTimeoutStorage(ISerializeObjects objectSerializer, ITransportMessageFactory transportMessageFactory)
        {
            this.objectSerializer = objectSerializer;
            this.transportMessageFactory = transportMessageFactory;
            connectionString = Settings.GetSetting<string>(SqlStorageConfiguration.StorageConnectionStringKey);
            CreateTableIfNecessary();
        }

        private void CreateTableIfNecessary()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(SqlCommands.CreateTimeoutTable, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Add(TimeoutData timeout)
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                using (var command = BuildAddCommand(connection, timeout))
                {
                    command.ExecuteNonQuery();
                }

                connection.Commit();
            }
        }

        public void Add(Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers)
        {
            var message = transportMessageFactory.BuildTransportMessage(correlationId, timeToLive, messages, headers);
            Add(new TimeoutData(message));
        }

        private SqlCommand BuildAddCommand(TransactionalSqlConnection connection, TimeoutData timoutData)
        {
            var command = connection.BuildCommand(SqlCommands.AddTimeout);
            command.CommandType = CommandType.Text;

            command.Parameters.AddWithValue("@Id", timoutData.MessageId);
            command.Parameters.AddWithValue("@Destination", timoutData.DestinationAddress);
            command.Parameters.AddWithValue("@Headers", objectSerializer.SerializeObject(timoutData.Headers));
            command.Parameters.AddWithValue("@Expires", timoutData.Expires);
            command.Parameters.AddWithValue("@Body", timoutData.Body);

            if (timoutData.CorrelationId != Guid.Empty)
            {
                command.Parameters.AddWithValue("@CorrelationId", timoutData.CorrelationId);
            }
            else
            {
                command.Parameters.AddWithValue("@CorrelationId", DBNull.Value);
            }

            return command;
        }

        public bool TryFetchNextTimeout(out TimeoutData timeoutData)
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            using (var command = connection.BuildCommand(SqlCommands.TryRemoveTimeout))
            {
                using (var dataReader = command.ExecuteReader())
                {
                    timeoutData = FoundTimeoutData(dataReader);
                }

                connection.Commit();

                return timeoutData != null;
            }
        }

        private TimeoutData FoundTimeoutData(SqlDataReader dataReader)
        {
            if (dataReader.Read())
            {
                return new TimeoutData
                {
                    MessageId = dataReader.GetGuid(messageIdIndex),
                    CorrelationId = dataReader.IsDBNull(correlationIdIndex) ? Guid.Empty : Guid.Parse(dataReader.GetString(correlationIdIndex)),
                    DestinationAddress = dataReader.GetString(destinationIndex),
                    Expires = dataReader.GetDateTime(timeToLiveIndex),
                    Headers = objectSerializer.DeserializeObject<Dictionary<string, string>>(dataReader.GetString(headersIndex)),
                    Body = dataReader.IsDBNull(bodyIndex) ? null : dataReader.GetSqlBinary(bodyIndex).Value
                };
            }

            return null;
        }
    }
}