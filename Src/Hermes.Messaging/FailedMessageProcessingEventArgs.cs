using System;

namespace Hermes.Messaging
{
    public class FailedMessageProcessingEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public TransportMessage Message { get; private set; }

        public FailedMessageProcessingEventArgs(Exception exception, TransportMessage message)
        {
            Exception = exception;
            Message = message;
        }
    }
}