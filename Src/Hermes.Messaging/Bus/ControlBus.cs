using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Bus
{
    public class ControlBus
    {
        private readonly ITransportMessages messageTransport;

        public ControlBus(ITransportMessages messageTransport)
        {
            this.messageTransport = messageTransport;
        }

        public void Send(Address address, params HeaderValue[] headers)
        {
            var outgoingMessage = OutgoingMessageContext.BuildControl(address, headers);
            messageTransport.SendMessage(outgoingMessage);
        }

        public void Broadcast(params HeaderValue[] headers)
        {
            var outgoingMessage = OutgoingMessageContext.BuildControl(headers);
            messageTransport.SendMessage(outgoingMessage);
        }
    }
}