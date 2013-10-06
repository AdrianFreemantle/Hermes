using System;

namespace Hermes.Messaging.Transports
{
    public interface IPublishMessages
    {
        bool Publish(params IEvent[] messages);
        bool Publish(Guid correlationId, params IEvent[] messages);
    }
}
