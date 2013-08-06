using System.Data;
using System.Data.SqlClient;

using Hermes.Configuration;

namespace Hermes.Transports.SqlServer
{
    public class SqlQueueCreator : ICreateQueues
    {
        const string CreateQueueSql =
            @"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'queue')
              BEGIN
                  EXEC( 'CREATE SCHEMA queue' );
              END
              
              IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[queue].[{0}]') AND type in (N'U'))
                  BEGIN
                    CREATE TABLE [queue].[{0}](
	                    [Id] [uniqueidentifier] NOT NULL,
	                    [CorrelationId] [varchar](255) NULL,
	                    [ReplyToAddress] [varchar](255) NULL,
	                    [Recoverable] [bit] NOT NULL,
	                    [Expires] [datetime] NULL,
	                    [Headers] [varchar](max) NOT NULL,
	                    [Body] [varbinary](max) NULL,
	                    [RowVersion] [bigint] IDENTITY(1,1) NOT NULL
                    ) ON [PRIMARY];                    

                    CREATE CLUSTERED INDEX [Index_RowVersion] ON [queue].[{0}] 
                    (
	                    [RowVersion] ASC
                    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
               END";

        private readonly string connectionString;

        public SqlQueueCreator()
        {
            connectionString = Settings.GetSetting<string>(SqlMessagingConfiguration.MessagingConnectionStringKey);
        }

        public void CreateQueueIfNecessary(Address address)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(string.Format(CreateQueueSql, address.Queue), connection))
                {
                    command.CommandType = CommandType.Text; 
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
