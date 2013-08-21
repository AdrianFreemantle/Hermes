using Hermes.Messaging;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageTransport : ITransportMessages
    {
        private readonly ISendMessages messageSender;
        private readonly IDequeueMessages messageReceiver;

        public MessageTransport(ISendMessages messageSender, IDequeueMessages messageReceiver)
        {
            this.messageSender = messageSender;
            this.messageReceiver = messageReceiver;
        }

        public void Dispose()
        {
            messageReceiver.Stop();
        }

        public void Start(Address queueAddress)
        {
            messageReceiver.Start(queueAddress);
        }

        public void Stop()
        {
            messageReceiver.Stop();
        }

        public void Send(MessageEnvelope message, Address recipient)
        {
            messageSender.Send(message, recipient);
        }
    }
}
