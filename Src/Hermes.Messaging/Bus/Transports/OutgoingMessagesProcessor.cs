using System.Collections.Generic;

using Hermes.Persistence;

namespace Hermes.Messaging.Bus.Transports
{
    public class OutgoingMessagesProcessor : IProcessOutgoingMessages, IUnitOfWork
    {
        readonly List<OutgoingMessage> outgoingMessages = new List<OutgoingMessage>();
        private readonly ISendMessages messageSender;

        public OutgoingMessagesProcessor(ISendMessages messageSender)
        {
            this.messageSender = messageSender;
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
            messageSender.Send(outgoingMessages);
        }

        public void Rollback()
        {
            outgoingMessages.Clear();
        }
    }
}