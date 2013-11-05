using System;

namespace Hermes.Messaging
{
    public interface IPublishMessages
    {
        bool Publish(IOutgoingMessageContext outgoingMessage);
    }
}
