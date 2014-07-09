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

        public void Send(Address address, HeaderValue[] headers)
        {
            Mandate.ParameterNotNull(address, "address");
            Mandate.ParameterNotNullOrEmpty(headers, "headers");

            var outgoingMessage = OutgoingMessageContext.BuildControl(address, headers);
            messageTransport.SendMessage(outgoingMessage);
        }
    }
}