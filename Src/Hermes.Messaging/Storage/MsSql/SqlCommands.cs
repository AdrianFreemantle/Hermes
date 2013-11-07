namespace Hermes.Messaging.Storage.MsSql
{
    internal class SqlCommands
    {
        public const string CreateSubscribtionTable =
            @"IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subscription]') AND type in (N'U'))
              BEGIN
                  CREATE TABLE [dbo].[Subscription](
                      [SubscriberEndpoint] [varchar](450) NOT NULL,
                      [MessageType] [varchar](450) NOT NULL,
                  PRIMARY KEY CLUSTERED 
                  (
                      [SubscriberEndpoint] ASC,
                      [MessageType] ASC
                  )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
              )
              END";

        public const string Subscribe =
            @"IF (SELECT COUNT(*) FROM [dbo].[Subscription]
                 WHERE [dbo].[Subscription].[SubscriberEndpoint] = @SubscriberEndpoint
                 AND [dbo].[Subscription].[MessageType] = @MessageType) = 0
             BEGIN
                 INSERT INTO [dbo].[Subscription]
                            ([SubscriberEndpoint]
                            ,[MessageType])
                      VALUES
                            (@SubscriberEndpoint
                            ,@MessageType)
             END";

        public const string Unsubscribe =
            @"DELETE FROM [dbo].[Subscription]
                     WHERE [dbo].[Subscription].[SubscriberEndpoint] = @SubscriberEndpoint
                     AND [dbo].[Subscription].[MessageType] = @MessageType";

        public const string GetSubscribers =
            @"SELECT [Subscription].[SubscriberEndpoint] FROM [dbo].[Subscription]
              WHERE [Subscription].[MessageType] = @MessageType";

        public const string CreateTimeoutTable =
             @"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'timeout')
              BEGIN
                  EXEC( 'CREATE SCHEMA timeout' );
              END

               IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[timeout].[{0}]') AND type in (N'U'))
               BEGIN
                 CREATE TABLE [timeout].[{0}](
	                 [Id] [uniqueidentifier] NOT NULL,
                     [CorrelationId] [varchar](255) NULL,
	                 [Destination] [varchar](255) NOT NULL,
                     [Expires] [datetime] NOT NULL,
	                 [Headers] [varchar](max) NOT NULL,
	                 [Body] [varbinary](max) NULL,
                     [RowVersion] [bigint] IDENTITY(1,1) NOT NULL
                 ) ON [PRIMARY];        
           
                 CREATE CLUSTERED INDEX [Index_RowVersion] ON [timeout].[{0}]
                 (
                    [RowVersion] ASC
                 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

                 CREATE INDEX [Index_Expires] ON [timeout].[{0}]
                 (
                    [Expires] ASC
                 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
             END";

        public const string AddTimeout =
            @"INSERT INTO [timeout].[{0}]' ([Id],[CorrelationId],[Destination],[Expires],[Headers],[Body]) 
              VALUES (@Id,@CorrelationId,@Destination,@Expires,@Headers,@Body)";

        public const string TryRemoveTimeout =
            @"WITH message AS (SELECT TOP(1) * FROM [timeout].[{0}] WITH (UPDLOCK, READPAST, ROWLOCK) WHERE [Expires] < GETUTCDATE() ORDER BY [Expires] ASC) 
              DELETE FROM message 
              OUTPUT deleted.Id, deleted.CorrelationId, deleted.Destination, deleted.Expires, deleted.Headers, deleted.Body;";
    }
}