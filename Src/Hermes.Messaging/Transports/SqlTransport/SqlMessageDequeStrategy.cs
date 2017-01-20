using System;
using System.Data;
using System.Data.SqlClient;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Serialization;

namespace Hermes.Messaging.Transports.SqlTransport
{
    public class SqlMessageDequeStrategy : IDequeueMessages
    {
        private readonly string connectionString;
        private readonly ISerializeObjects serializer;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (SqlMessageDequeStrategy));
        private static readonly string DequeueSql = String.Format(SqlCommands.Dequeue, Address.Local);

        public SqlMessageDequeStrategy(ISerializeObjects serializer)
        {
            Logger.Debug("Ctor");
            connectionString = Settings.GetSetting(SqlTransportConfiguration.MessagingConnectionStringKey);
            this.serializer = serializer;
        }

        public TransportMessage Dequeue()
        {
            try
            {
                return TryDequeue();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while attempting to dequeue message: {0}", ex.GetFullExceptionMessage());
                throw;
            }
        }

        private TransportMessage TryDequeue()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(DequeueSql, connection))
                {
                    return FetchNextMessage(command);
                }
            }
        }

        private TransportMessage FetchNextMessage(SqlCommand command)
        {
            using (var dataReader = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (dataReader.Read())
                {
                    TransportMessage message = dataReader.BuildTransportMessage(serializer);
                    Logger.Debug("Dequeued message {0}", message.MessageId);
                    return message;
                }
            }

            return TransportMessage.Undefined;
        }
    }
}