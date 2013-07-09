using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Hermes.Configuration;
using Hermes.Subscriptions;

namespace Hermes.Transports.SqlServer
{
    public class SqlSubscriptionStorage : ISubscriptionStorage
    {
        const string CreateSubscribtionTableSql =
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

        private readonly string connectionString;

        public SqlSubscriptionStorage()
        {
            connectionString = Settings.GetSetting<string>(SqlMessagingSettings.MessagingConnectionStringKey);
        }

        public void Subscribe(Address client, IEnumerable<Type> messageTypes)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Address client, IEnumerable<Type> messageTypes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<Type> messageTypes)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            CreateSubcribtionTableIfNecessary();
        }

        public void CreateSubcribtionTableIfNecessary()
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
    }
}