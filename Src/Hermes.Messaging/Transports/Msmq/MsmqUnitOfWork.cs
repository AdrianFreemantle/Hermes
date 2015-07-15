using System;
using System.Messaging;
using System.Threading;

namespace Hermes.Messaging.Transports.Msmq
{
    public class MsmqUnitOfWork : IDisposable
    {
        readonly ThreadLocal<MessageQueueTransaction> currentTransaction = new ThreadLocal<MessageQueueTransaction>();

        public MessageQueueTransaction Transaction
        {
            get { return currentTransaction.Value; }
        }

        public void Dispose()
        {
            //Injected
        }

        public void SetTransaction(MessageQueueTransaction msmqTransaction)
        {
            currentTransaction.Value = msmqTransaction;
        }

        public bool HasActiveTransaction()
        {
            return currentTransaction.IsValueCreated;
        }

        public void ClearTransaction()
        {
            currentTransaction.Value = null;
        }
    }
}