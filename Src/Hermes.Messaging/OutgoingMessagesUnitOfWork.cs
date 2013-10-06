using System.Collections.Generic;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public class OutgoingMessagesUnitOfWork : IProcessOutgoingMessages
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
    }
}