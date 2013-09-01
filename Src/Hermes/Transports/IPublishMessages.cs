using System;
using System.Collections.Generic;

using Hermes.Messaging;

namespace Hermes.Transports
{
    public interface IPublishMessages
    {
       bool Publish(TransportMessage transportMessage, IEnumerable<Type> eventTypes);
    }
}
