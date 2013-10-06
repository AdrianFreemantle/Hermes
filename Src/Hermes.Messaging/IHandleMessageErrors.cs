using System;

namespace Hermes.Messaging
{
    public interface IHandleMessageErrors
    {
        void Handle(TransportMessage envelope, Exception ex);
        void RemoveRetryHeaders(TransportMessage envelope);
    }
}
