using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public class CompletedMessageProcessingEventArgs : EventArgs
    {
        readonly TransportMessage transportMessage;
        private readonly object[] messages;

        public TransportMessage TransportMessage
        {
            get { return transportMessage; }
        }

        public IEnumerable<object> Messages
        {
            get { return messages; }
        }

        public CompletedMessageProcessingEventArgs(TransportMessage transportMessage, object[] messages)
        {
            this.transportMessage = transportMessage;
            this.messages = messages;
        }
    }
}