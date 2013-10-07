using System.Collections.Generic;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public class OutgoingMessagesUnitOfWork : IProcessOutgoingMessages, IManageUnitOfWork
    {
        readonly List<OutgoingMessage> outgoingMessages = new List<OutgoingMessage>();
        private readonly ISendMessages messageSender;

        public OutgoingMessagesUnitOfWork(ISendMessages messageSender)
        {
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
            outgoingMessages.Clear();
        }

        public void Commit()
        {
            //no-op as we send outgoing messages outside of the client process transaction scope
        }

        public void Rollback()
        {
            outgoingMessages.Clear();
        }
    }
}