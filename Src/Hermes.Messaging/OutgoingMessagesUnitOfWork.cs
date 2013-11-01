using System;
using System.Collections.Generic;

using Hermes.Messaging.Transports;
using Hermes.Persistence;

namespace Hermes.Messaging
{
    public class OutgoingMessagesUnitOfWork : IProcessOutgoingMessages, IUnitOfWork
    {
        readonly List<OutgoingMessage> outgoingMessages = new List<OutgoingMessage>();
        private readonly ISendMessages messageSender;

        public OutgoingMessagesUnitOfWork(ISendMessages messageSender)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Starting new OutgoingMessagesUnitOfWork {0}", GetHashCode()));
            this.messageSender = messageSender;
        }

        public void Send()
        {
            messageSender.Send(outgoingMessages);
        }

        public void Add(OutgoingMessage message)
        {
            outgoingMessages.Add(message);
        }

        public void Add(IEnumerable<OutgoingMessage> messages)
        {
            outgoingMessages.AddRange(messages);
        }

        public void Dispose()
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Dispose OutgoingMessagesUnitOfWork {0}", GetHashCode()));
            outgoingMessages.Clear();
        }

        public void Commit()
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Committing OutgoingMessagesUnitOfWork {0}", GetHashCode()));
            messageSender.Send(outgoingMessages);
        }

        public void Rollback()
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Rolling back OutgoingMessagesUnitOfWork {0}", GetHashCode()));
            outgoingMessages.Clear();
        }
    }
}