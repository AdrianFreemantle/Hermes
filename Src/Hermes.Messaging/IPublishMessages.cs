using System;

namespace Hermes.Messaging
{
    public interface IPublishMessages
    {
        bool Publish(params object[] messages);
        bool Publish(Guid correlationId, params object[] messages);
    }
}
