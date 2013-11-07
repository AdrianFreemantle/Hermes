using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Hermes.Messaging.Configuration;
using Hermes.Messaging.Timeouts;
using Hermes.Serialization;
using Hermes.Sql;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SqlTimeoutStorage : IPersistTimeouts
    {       
        private readonly ISerializeObjects objectSerializer;
        private readonly ISerializeMessages messageSerializer;
        private readonly string connectionString;

        const int messageIdIndex = 0;
        const int correlationIdIndex = 1;
        const int destinationIndex = 2;
        const int timeToLiveIndex = 3;
        const int headersIndex = 4;
        const int bodyIndex = 5;

        public SqlTimeoutStorage(ISerializeObjects objectSerializer, ISerializeMessages messageSerializer)
        {
            this.objectSerializer = objectSerializer;
            this.messageSerializer = messageSerializer;
            connectionString = Settings.GetSetting<string>(SqlStorageConfiguration.StorageConnectionStringKey);
            CreateTableIfNecessary();
        }

        private void CreateTableIfNecessary()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(String.Format(SqlCommands.CreateTimeoutTable, Address.Local), connection))
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
            Mandate.ParameterNotNullOrEmpty(messages, "messages");

            var serializedMessages = messageSerializer.Serialize(messages);

            var timeoutData = new TimeoutData
            {
                MessageId = SequentialGuid.New(),
                CorrelationId = correlationId,
                Body = serializedMessages,
                DestinationAddress = Address.Local.ToString(),
                Expires = DateTime.MaxValue,
                Headers = headers
            };

            Add(timeoutData);
        }

        private SqlCommand BuildAddCommand(TransactionalSqlConnection connection, TimeoutData timoutData)
        {
            var command = connection.BuildCommand(String.Format(SqlCommands.AddTimeout, Address.Local));
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
            using (var command = connection.BuildCommand(String.Format(SqlCommands.TryRemoveTimeout, Address.Local)))
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