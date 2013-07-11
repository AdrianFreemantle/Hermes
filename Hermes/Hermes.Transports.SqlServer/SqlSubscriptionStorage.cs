using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Hermes.Configuration;
using Hermes.Subscriptions;

namespace Hermes.Transports.SqlServer
{
    public class SqlSubscriptionStorage : ISubscriptionStorage
    {
        private readonly string connectionString;

        private const string CreateSubscribtionTableSql =
            @"IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Message.Subscription]') AND type in (N'U'))
              BEGIN
                  CREATE TABLE [dbo].[Message.Subscription](
                      [SubscriberEndpoint] [varchar](450) NOT NULL,
                      [MessageType] [varchar](450) NOT NULL,
                  PRIMARY KEY CLUSTERED 
                  (
                      [SubscriberEndpoint] ASC,
                      [MessageType] ASC
                  )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
              )
              END";

        private const string SubscribeSql =
            @"IF (SELECT COUNT(*) FROM [dbo].[Message.Subscription]
                 WHERE [dbo].[Message.Subscription].[SubscriberEndpoint] = @SubscriberEndpoint
                 AND [dbo].[Message.Subscription].[MessageType] = @MessageType) = 0
             BEGIN
                 INSERT INTO [dbo].[Message.Subscription]
                            ([SubscriberEndpoint]
                            ,[MessageType])
                      VALUES
                            (@Subscription
                            ,@MessageType)
             END";

        private const string UnsubscribeSql =
            @"DELETE FROM [dbo].[Message.Subscription]
                     WHERE [dbo].[Message.Subscription].[SubscriberEndpoint] = @Subscription
                     AND [dbo].[Message.Subscription].[MessageType] = @MessageType";

        private const string GetSubscribersSql =
            @"SELECT [Message.Subscription].[SubscriberEndpoint] FROM [dbo].[Message.Subscription]
              WHERE [Message.Subscription].[MessageType] = @MessageType";

        public SqlSubscriptionStorage()
        {
            connectionString = Settings.GetSetting<string>(SqlMessagingSettings.MessagingConnectionStringKey);
            CreateSubcribtionTableIfNecessary();
        }

        private void CreateSubcribtionTableIfNecessary()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(CreateSubscribtionTableSql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Subscribe(Address client, IEnumerable<Type> messageTypes)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                foreach (var messageType in messageTypes)
                {
                    var subscriberEndpointParam = new SqlParameter("SubscriberEndpoint", client.ToString());
                    var messageTypeParam = new SqlParameter("MessageType", messageType.FullName);

                    var command = connection.BuildCommand(SubscribeSql, subscriberEndpointParam, messageTypeParam);
                    command.ExecuteNonQuery();
                }

                connection.Commit();
                scope.Complete();
            }
        }

        public void Unsubscribe(Address client, IEnumerable<Type> messageTypes)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                foreach (var messageType in messageTypes)
                {
                    var subscriberEndpointParam = new SqlParameter("SubscriberEndpoint", client.ToString());
                    var messageTypeParam = new SqlParameter("MessageType", messageType.FullName);

                    var command = connection.BuildCommand(UnsubscribeSql, subscriberEndpointParam, messageTypeParam);
                    command.ExecuteNonQuery();
                }

                connection.Commit();
                scope.Complete();
            }
        }

        public IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<Type> messageTypes)
        {
            var subscribers = new List<Address>();

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            using (var connection = TransactionalSqlConnection.Begin(connectionString))
            {
                foreach (var messageType in messageTypes)
                {
                    var messageTypeParam = new SqlParameter("MessageType", messageType.FullName);
                    var command = connection.BuildCommand(GetSubscribersSql, messageTypeParam);

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