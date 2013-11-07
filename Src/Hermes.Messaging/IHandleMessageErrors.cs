using System;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public interface IHandleMessageErrors
    {
        void Handle(TransportMessage transportMessage, Exception ex);
    }
}
