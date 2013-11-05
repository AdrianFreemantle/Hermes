using System;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    public class MessageProcessingEventArgs : EventArgs
    {
        public TransportMessage TransportMessage { get; private set; }

        public MessageProcessingEventArgs(TransportMessage transportMessage)
        {
            TransportMessage = transportMessage;
        }
    }
}