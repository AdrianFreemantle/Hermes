using System.Data;
using System.Data.SqlClient;

namespace Hermes.Transports.SqlServer
{
    public class SqlServerQueueCreator : ICreateQueues
    {
        private readonly string connectionString;

        public SqlServerQueueCreator(string connectionString)
        {
            Mandate.ParameterNotNullOrEmpty(connectionString, "connectionString");
            this.connectionString = connectionString;
        }

        const string createQueueSql =
            @"IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[@queue]') AND type in (N'U'))
                  BEGIN
                    CREATE TABLE [dbo].[@queue](
	                    [Id] [uniqueidentifier] NOT NULL,
	                    [CorrelationId] [varchar](255) NULL,
	                    [ReplyToAddress] [varchar](255) NULL,
	                    [Recoverable] [bit] NOT NULL,
	                    [Expires] [datetime] NULL,
	                    [Headers] [varchar](max) NOT NULL,
	                    [Body] [varbinary](max) NULL,
	                    [RowVersion] [bigint] IDENTITY(1,1) NOT NULL
                    ) ON [PRIMARY];                    

                    CREATE CLUSTERED INDEX [Index_RowVersion] ON [dbo].[@queue] 
                    (
	                    [RowVersion] ASC
                    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                    
                  END";

        public void CreateQueueIfNecessary(Address address, string account)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(createQueueSql, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@queue", address.Queue);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
