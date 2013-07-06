﻿using System.Data;
using System.Data.SqlClient;

namespace Hermes.Transports.SqlServer
{
    public class SqlServerQueueCreator : ICreateQueues
    {
        const string createQueueSql =
            @"IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U'))
                  BEGIN
                    CREATE TABLE [dbo].[{0}](
	                    [Id] [uniqueidentifier] NOT NULL,
	                    [CorrelationId] [varchar](255) NULL,
	                    [ReplyToAddress] [varchar](255) NULL,
	                    [Recoverable] [bit] NOT NULL,
	                    [Expires] [datetime] NULL,
	                    [Headers] [varchar](max) NOT NULL,
	                    [Body] [varbinary](max) NULL,
	                    [RowVersion] [bigint] IDENTITY(1,1) NOT NULL
                    ) ON [PRIMARY];                    

                    CREATE CLUSTERED INDEX [Index_RowVersion] ON [dbo].[{0}] 
                    (
	                    [RowVersion] ASC
                    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                    
                  END";

        private readonly string connectionString;

        public SqlServerQueueCreator()
        {
            connectionString = Configuration.GetSetting<string>(Configuration.ConnectionString);
        }

        public void CreateQueueIfNecessary(Address address)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(string.Format(createQueueSql, address.Queue), connection))
                {
                    command.CommandType = CommandType.Text; 
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
