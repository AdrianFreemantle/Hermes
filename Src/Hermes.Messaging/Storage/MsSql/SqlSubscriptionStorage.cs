using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports.SqlTransport;
using IsolationLevel = System.Data.IsolationLevel;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SqlSubscriptionStorage : IStoreSubscriptions
    {
        private readonly string connectionString;
        private readonly SubscriptionCache subscriptionCache;

        public SqlSubscriptionStorage()
        {
            subscriptionCache = new SubscriptionCache();
            connectionString = Settings.GetSetting<string>(SqlTransportConfiguration.MessagingConnectionStringKey);
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
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var messageType in messageTypes)
                {
                    var command = new SqlCommand(SqlCommands.Subscribe, connection);
                    command.Parameters.Add(new SqlParameter("SubscriberEndpoint", client.ToString()));
                    command.Parameters.Add(new SqlParameter("MessageType", messageType.FullName));
                    command.ExecuteNonQuery();
                }

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
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var messageType in messageTypes)
                {
                    var command = new SqlCommand(SqlCommands.Unsubscribe, connection);
                    command.Parameters.Add(new SqlParameter("SubscriberEndpoint", client.ToString()));
                    command.Parameters.Add(new SqlParameter("MessageType", messageType.FullName));
                    command.ExecuteNonQuery();
                }

                scope.Complete();
            }
        }

        public IEnumerable<Address> GetSubscribersForMessageTypes(IEnumerable<Type> messageTypes)
        {
            var contracts = messageTypes.ToArray();
            IEnumerable<Address> subscribers;

            if (!subscriptionCache.TryGetSubscribers(contracts, out subscribers))
            {
                UpdateSubsciptions(contracts);
                subscriptionCache.TryGetSubscribers(contracts, out subscribers);
            }

            return subscribers ?? new Address[0];
        }

        private void UpdateSubsciptions(IEnumerable<Type> contracts)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var messageType in contracts)
                {
                    var subscribers = new List<Address>();
                    var command = new SqlCommand(SqlCommands.GetSubscribers, connection);
                    command.Parameters.Add(new SqlParameter("MessageType", messageType.FullName));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var subscriber = Address.Parse(reader.GetString(0));
                            subscribers.Add(subscriber);
                        }
                    }

                    subscriptionCache.UpdateSubscribers(messageType, subscribers, TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
