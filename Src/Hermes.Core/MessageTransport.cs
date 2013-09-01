using System;

using Hermes.Messaging;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageTransport : ITransportMessages
    {
        private readonly ISendMessages messageSender;
        private readonly IReceiveMessages messageReceiver;

        public MessageTransport(ISendMessages messageSender, IReceiveMessages messageReceiver)
        {
            this.messageSender = messageSender;
            this.messageReceiver = messageReceiver;
        }

        public void Dispose()
        {
            messageReceiver.Stop();
        }

        public void Start()
        {
            messageReceiver.Start();
        }

        public void Stop()
        {
            messageReceiver.Stop();
        }

        public void Send(TransportMessage transportMessage, Address recipient)
        {
            messageSender.Send(transportMessage, recipient);
        }
    }
}
