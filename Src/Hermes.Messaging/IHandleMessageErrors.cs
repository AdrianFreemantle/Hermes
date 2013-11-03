using System;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    public interface IHandleMessageErrors
    {
        void Handle(TransportMessage transportMessage, Exception ex);
        void RemoveRetryHeaders(TransportMessage envelope);
    }
}
