using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Hermes.Configuration;
using Hermes.Core.Deferment;
using Hermes.Messaging;
using Hermes.Serialization;

namespace Hermes.Storage.SqlServer
{
    public class SqlTimeoutStorage : IPersistTimeouts
    {       
        private readonly ISerializeObjects objectSerializer;
        private readonly string connectionString;

        public SqlTimeoutStorage(ISerializeObjects objectSerializer)
        {
            this.objectSerializer = objectSerializer;
            connectionString = Settings.GetSetting<string>(SqlStorageConfiguration.StorageConnectionStringKey);
            CreateTableIfNecessary();
        }

        public void CreateTableIfNecessary()
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

        public IEnumerable<long> GetExpired()
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = SqlCommands.GetExpired;
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@Expiry", DateTime.UtcNow);

                connection.Open();

                using (var dataReader = command.ExecuteReader())
                {
                    return GetExpiredIds(dataReader);
                }
            }
        }

        private static IEnumerable<long> GetExpiredIds(SqlDataReader dataReader)
        {
            var results = new List<long>();

            while (dataReader.Read())
            {
                results.Add((long)dataReader.GetSqlInt64(0));
            }

            return results;
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

        private SqlCommand BuildAddCommand(TransactionalSqlConnection connection, TimeoutData timoutData)
        {
            var command = connection.BuildCommand(SqlCommands.AddTimeout);
            command.CommandType = CommandType.Text;

            command.Parameters.AddWithValue("@Id", timoutData.MessageId);
            command.Parameters.AddWithValue("@Destination", timoutData.Destination.ToString());
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

        public bool TryRemove(long timeoutId, out TimeoutData timeoutData)
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            using (var command = connection.BuildCommand(SqlCommands.TryRemoveTimeout))
            {
                command.Parameters.AddWithValue("@Id", timeoutId);

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
                return  new TimeoutData
                {
                    MessageId = dataReader.GetGuid(0),
                    CorrelationId = dataReader.IsDBNull(1) ? Guid.Empty : Guid.Parse(dataReader.GetString(1)),
                    Destination = Address.Parse(dataReader.GetString(2)),
                    Expires = dataReader.GetDateTime(3),
                    Headers = objectSerializer.DeserializeObject<Dictionary<string, string>>(dataReader.GetString(4)),
                    Body = dataReader.IsDBNull(5) ? null : dataReader.GetSqlBinary(5).Value
                };
            }

            return null;
        }
    }
}