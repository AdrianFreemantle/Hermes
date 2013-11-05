using System;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    public class MessageProcessingProcessingErrorEventArgs : MessageProcessingEventArgs
    {
        public Exception Error { get; protected set; }

        public MessageProcessingProcessingErrorEventArgs(TransportMessage transportMessage, Exception ex)
            : base(transportMessage)
        {
            Error = ex;
        }
    }
}