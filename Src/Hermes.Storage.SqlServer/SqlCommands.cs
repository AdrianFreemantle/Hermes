namespace Hermes.Storage.SqlServer
{
    internal class SqlCommands
    {
        public const string CreateSubscribtionTable =
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

        public const string Subscribe =
            @"IF (SELECT COUNT(*) FROM [dbo].[Message.Subscription]
                 WHERE [dbo].[Message.Subscription].[SubscriberEndpoint] = @SubscriberEndpoint
                 AND [dbo].[Message.Subscription].[MessageType] = @MessageType) = 0
             BEGIN
                 INSERT INTO [dbo].[Message.Subscription]
                            ([SubscriberEndpoint]
                            ,[MessageType])
                      VALUES
                            (@SubscriberEndpoint
                            ,@MessageType)
             END";

        public const string Unsubscribe =
            @"DELETE FROM [dbo].[Message.Subscription]
                     WHERE [dbo].[Message.Subscription].[SubscriberEndpoint] = @SubscriberEndpoint
                     AND [dbo].[Message.Subscription].[MessageType] = @MessageType";

        public const string GetSubscribers =
            @"SELECT [Message.Subscription].[SubscriberEndpoint] FROM [dbo].[Message.Subscription]
              WHERE [Message.Subscription].[MessageType] = @MessageType";

        public const string CreateTimeoutTable =
             @"IF NOT  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Message.TimeoutData]') AND type in (N'U'))
               BEGIN
                 CREATE TABLE [dbo].[Message.TimeoutData](
	                 [Id] [uniqueidentifier] NOT NULL,
                     [CorrelationId] [varchar](255) NULL,
	                 [Destination] [varchar](255) NOT NULL,
                     [Expires] [datetime] NOT NULL,
	                 [Headers] [varchar](max) NOT NULL,
	                 [Body] [varbinary](max) NULL
                 ) ON [PRIMARY];        
           
                 CREATE CLUSTERED INDEX [Index_Expires] ON [dbo].[Message.TimeoutData]
                 (
	                 [Expires] ASC
                 )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
             END";

        public const string AddTimeout =
            @"INSERT INTO [dbo].[Message.TimeoutData] ([Id],[CorrelationId],[Destination],[Expires],[Headers],[Body]) 
              VALUES (@Id,@CorrelationId,@Destination,@Expires,@Headers,@Body)";

        public const string TryRemoveTimeout =
            @"WITH message AS (SELECT TOP(1) * FROM [dbo].[Message.TimeoutData] WITH (UPDLOCK, READPAST, ROWLOCK) WHERE [Expires] < GETUTCDATE() ORDER BY [Expires] ASC) 
              DELETE FROM message 
              OUTPUT deleted.Id, deleted.CorrelationId, deleted.Destination, deleted.Expires, deleted.Headers, deleted.Body;";
    }
}