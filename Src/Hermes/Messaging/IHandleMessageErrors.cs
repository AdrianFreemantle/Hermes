using System;

namespace Hermes.Messaging
{
    public interface IHandleMessageErrors
    {
        void Handle(MessageEnvelope envelope, Exception ex);
        void RemoveRetryHeaders(MessageEnvelope envelope);
    }
}
