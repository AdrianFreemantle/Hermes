using System;

namespace Hermes.Messaging
{
    public class FailedMessageProcessingEventArgs : EventArgs
    {
        readonly Exception exception;

        public Exception Exception
        {
            get { return exception; }
        }

        public FailedMessageProcessingEventArgs(Exception exception)
        {
            this.exception = exception;
        }
    }
}