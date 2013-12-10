using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging
{
    public interface IPublishMessages
    {
        bool Publish(OutgoingMessageContext outgoingMessage);
    }
}
