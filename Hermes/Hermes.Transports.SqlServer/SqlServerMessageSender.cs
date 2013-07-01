using System;
using System.Data.SqlClient;
using System.Threading;

using Hermes.Serialization;

namespace Hermes.Transports.SqlServer
{
    public class SqlServerMessageSender : ISendMessages, IDisposable
    {
        private const string SqlSend = @"INSERT INTO [{0}] ([Id],[CorrelationId],[ReplyToAddress],[Recoverable],[Expires],[Headers],[Body]) 
                                         VALUES (@Id,@CorrelationId,@ReplyToAddress,@Recoverable,@Expires,@Headers,@Body)";

        private readonly ISerializeMessages serializer;
        private readonly ThreadLocal<SqlTransaction> currentTransaction = new ThreadLocal<SqlTransaction>();
        
        private bool disposed;

        public SqlServerMessageSender(ISerializeMessages serializer)
        {
            this.serializer = serializer;
        }

        ~SqlServerMessageSender()
        {
            Dispose(false);
        }

        public void Send(EnvelopeMessage message, Address address)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
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
                currentTransaction.Dispose();
            }

            disposed = true;
        }
    }
}
