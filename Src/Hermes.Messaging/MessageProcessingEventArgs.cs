using System;

using Hermes.Messaging.Transports;

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