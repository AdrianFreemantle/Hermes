namespace Hermes.Messaging.Bus.Transports.SqlTransport
{
    internal static class SqlCommands
    {
        public const string Send = 
            @"INSERT INTO [queue].[{0}] ([Id],[CorrelationId],[ReplyTo],[Expires],[Headers],[Body]) 
            VALUES (@Id,@CorrelationId,@ReplyTo,@Expires,@Headers,@Body)";

        public const string Dequeue =
            @"WITH message AS (SELECT TOP(1) * FROM [queue].[{0}] WITH (UPDLOCK, READPAST, ROWLOCK) ORDER BY [RowVersion] ASC) 
            DELETE FROM message 
            OUTPUT deleted.Id, deleted.CorrelationId, deleted.ReplyTo, deleted.Expires, deleted.Headers, deleted.Body;";

        public const string CreateQueue =
            @"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'queue')
              BEGIN
                  EXEC( 'CREATE SCHEMA queue' );
              END
              
              IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[queue].[{0}]') AND type in (N'U'))
                  BEGIN
                    CREATE TABLE [queue].[{0}](
	                    [Id] [uniqueidentifier] NOT NULL,
	                    [CorrelationId] [varchar](255) NULL,
                        [ReplyTo] [varchar](450) NULL,
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

        public const string PurgeQueue =
            @"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[queue].[{0}]') AND type in (N'U'))
              BEGIN
                    DELETE FROM [queue].[{0}]
              END";
    }
}
