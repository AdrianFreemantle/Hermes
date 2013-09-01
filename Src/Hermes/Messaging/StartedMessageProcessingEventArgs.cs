using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public class StartedMessageProcessingEventArgs : EventArgs
    {
        readonly TransportMessage transportMessage;
        private readonly object[] messages;

        public TransportMessage TransportMessage
        {
            get { return transportMessage; }
        }

        public IReadOnlyCollection<object> Messages
        {
            get { return messages; }
        }

        public StartedMessageProcessingEventArgs(TransportMessage transportMessage, object[] messages)
        {
            this.transportMessage = transportMessage;
            this.messages = messages;
        }
    }
}
