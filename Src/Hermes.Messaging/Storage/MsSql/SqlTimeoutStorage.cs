using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Hermes.Messaging.Configuration;
using Hermes.Messaging.Timeouts;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.Serialization;
using Hermes.Messaging.Serialization;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SqlTimeoutStorage : IPersistTimeouts
    {       
        private readonly ISerializeObjects objectSerializer;
        private readonly ISerializeMessages messageSerializer;
        private readonly string connectionString;

        const int MessageIdIndex = 0;
        const int CorrelationIdIndex = 1;
        const int DestinationIndex = 2;
        const int TimeToLiveIndex = 3;
        const int HeadersIndex = 4;
        const int BodyIndex = 5;

        public SqlTimeoutStorage(ISerializeObjects objectSerializer, ISerializeMessages messageSerializer)
        {
            this.objectSerializer = objectSerializer;
            this.messageSerializer = messageSerializer;
            connectionString = Settings.GetSetting<string>(SqlTransportConfiguration.MessagingConnectionStringKey);
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

        public void Purge()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                using (var command = new SqlCommand(String.Format(SqlCommands.Purge, Address.Local), connection, transaction))
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public void Add(TimeoutData timeout)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = BuildAddCommand(connection, timeout))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Add(Guid correlationId, TimeSpan timeToLive, object message, IDictionary<string, string> headers)
        {
            Mandate.ParameterNotNull(message, "messages");

            var serializedMessages = messageSerializer.Serialize(message);

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

        private SqlCommand BuildAddCommand(SqlConnection connection, TimeoutData timoutData)
        {
            var command = connection.CreateCommand();
            command.CommandText = String.Format(SqlCommands.AddTimeout, Address.Local);
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
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(String.Format(SqlCommands.TryRemoveTimeout, Address.Local), connection))
                {
                    using (var dataReader = command.ExecuteReader())
                    {
                        timeoutData = FoundTimeoutData(dataReader);
                        return timeoutData != null;
                    }
                }
            }
        }

        public void Remove(Guid correlationId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(String.Format(SqlCommands.Remove, Address.Local), connection))
                {
                    command.Parameters.Add(new SqlParameter("correlationId", correlationId));
                    command.ExecuteNonQuery();
                }
            }
        }

        private TimeoutData FoundTimeoutData(SqlDataReader dataReader)
        {
            if (dataReader.Read())
            {
                return new TimeoutData
                {
                    MessageId = dataReader.GetGuid(MessageIdIndex),
                    CorrelationId = dataReader.IsDBNull(CorrelationIdIndex) ? Guid.Empty : Guid.Parse(dataReader.GetString(CorrelationIdIndex)),
                    DestinationAddress = dataReader.GetString(DestinationIndex),
                    Expires = dataReader.GetDateTime(TimeToLiveIndex),
                    Headers = objectSerializer.DeserializeObject<Dictionary<string, string>>(dataReader.GetString(HeadersIndex)),
                    Body = dataReader.IsDBNull(BodyIndex) ? null : dataReader.GetSqlBinary(BodyIndex).Value
                };
            }

            return null;
        }
    }
}