namespace Hermes.Messaging.Bus.Transports
{
    public class OutgoingMessage
    {
        public TransportMessage TransportMessage { get; private set; }
        public Address Address { get; private set; }

        public OutgoingMessage(TransportMessage transportMessage, Address address)
        {
            TransportMessage = transportMessage;
            Address = address;
        }
    }
}