using System;

namespace Hermes.Transports
{
    public interface IPublishMessages
    {
        bool Publish(params object[] messages);
    }
}
