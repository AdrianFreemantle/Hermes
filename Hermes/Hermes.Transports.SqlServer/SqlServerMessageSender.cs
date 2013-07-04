using System;
using System.Data;
using System.Data.SqlClient;

using Hermes.Serialization;

namespace Hermes.Transports.SqlServer
{
    public class SqlServerMessageSender : ISendMessages, IDisposable
    {
        private const string SqlSend = @"INSERT INTO [@queue] ([Id],[CorrelationId],[ReplyToAddress],[Recoverable],[Expires],[Headers],[Body]) 
                                         VALUES (@Id,@CorrelationId,@ReplyToAddress,@Recoverable,@Expires,@Headers,@Body)";

        private readonly ISerializeObjects objectSerializer;
        private bool disposed;

        public string ConnectionString { get; set; }

        public SqlServerMessageSender(ISerializeObjects objectSerializer)
        {
            this.objectSerializer = objectSerializer;
        }

        ~SqlServerMessageSender()
        {
            Dispose(false);
        }

        public void Send(MessageEnvelope message, Address address)
        {
            using (var transactionalConnection = new TransactionalSqlConnection(ConnectionString))
            {
                using (var command = BuildSendCommand(transactionalConnection, message, address))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private SqlCommand BuildSendCommand(TransactionalSqlConnection connection, MessageEnvelope message, Address address)
        {
            var command = connection.BuildCommand(SqlSend);
            command.CommandType = CommandType.Text;

            command.Parameters.AddWithValue("@queue", address.Queue);
            command.Parameters.AddWithValue("@Id", message.MessageId);
            command.Parameters.AddWithValue("@CorrelationId", message.CorrelationId);
            command.Parameters.AddWithValue("@ReplyToAddress", message.ReturnAddress.ToString());
            command.Parameters.AddWithValue("@Recoverable", message.Recoverable);
            command.Parameters.AddWithValue("@Headers", objectSerializer.SerializeObject(message.Headers));
            command.Parameters.AddWithValue("Body", message.Body);

            if (message.HasExpiryTime)
            {
                command.Parameters.AddWithValue("Expires", message.ExpiryTime);
            }
            else
            {
                command.Parameters.AddWithValue("Expires", DBNull.Value);
            }

            return command;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed resources.
                //currentTransaction.Dispose();
            }

            disposed = true;
        }
    }
}
