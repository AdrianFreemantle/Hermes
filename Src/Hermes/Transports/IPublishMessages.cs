using System;
using System.Collections.Generic;

namespace Hermes.Transports
{
    public interface IPublishMessages
    {
       bool Publish(MessageEnvelope message, IEnumerable<Type> eventTypes);
    }
}
