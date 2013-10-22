using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SqlSubscriptionStorage : IStoreSubscriptions
    {
        private readonly string connectionString;

        public SqlSubscriptionStorage()
        {
            connectionString = Settings.GetSetting<string>(SqlStorageConfiguration.StorageConnectionStringKey);
            CreateSubcribtionTableIfNecessary();
        }

        private void CreateSubcribtionTableIfNecessary()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(SqlCommands.CreateSubscribtionTable, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Subscribe(Address client, params Type[] messageTypes)
        {
            if (messageTypes == null || messageTypes.Length == 0)
            {
                return;
            }

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                foreach (var messageType in messageTypes)
                {
                    var subscriberEndpointParam = new SqlParameter("SubscriberEndpoint", client.ToString());
                    var messageTypeParam = new SqlParameter("MessageType", messageType.FullName);

                    var command = connection.BuildCommand(SqlCommands.Subscribe, subscriberEndpointParam, messageTypeParam);
                    command.ExecuteNonQuery();
                }

                connection.Commit();
                scope.Complete();
            }
        }

        public void Unsubscribe(Address client, params Type[] messageTypes)
        {
            if (messageTypes == null || messageTypes.Length == 0)
            {
                return;
            }

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                foreach (var messageType in messageTypes)
                {
                    var subscriberEndpointParam = new SqlParameter("SubscriberEndpoint", client.ToString());
                    var messageTypeParam = new SqlParameter("MessageType", messageType.FullName);

                    var command = connection.BuildCommand(SqlCommands.Unsubscribe, subscriberEndpointParam, messageTypeParam);
                    command.ExecuteNonQuery();
                }

                connection.Commit();
                scope.Complete();
            }
        }

        public IEnumerable<Address> GetSubscribersForMessageTypes(IEnumerable<Type> messageTypes)
        {
            var subscribers = new List<Address>();

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                foreach (var messageType in messageTypes)
                {
                    var messageTypeParam = new SqlParameter("MessageType", messageType.FullName);
                    var command = connection.BuildCommand(SqlCommands.GetSubscribers, messageTypeParam);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subscribers.Add(Address.Parse(reader.GetString(0)));
                        }
                    }
                }

                connection.Commit();
                scope.Complete();
            }

            return subscribers.Distinct();
        }
    }
}