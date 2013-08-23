using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IMessageBus  
    {
        void Send(ICollection<object> commands);
        void Send(Address address, ICollection<object> commands);
        void Send(Address address, Guid corrolationId, ICollection<object> commands);

        void Send(object command);
        void Send(Address address, object command);
        void Send(Address address, Guid corrolationId, object command);

        void Defer(TimeSpan delay, ICollection<object> commands);
        void Defer(TimeSpan delay, Guid corrolationId, ICollection<object> commands);

        void Defer(TimeSpan delay, object command);
        void Defer(TimeSpan delay, Guid corrolationId, object command);

        void Publish(object @event);
        void Publish(ICollection<object> events);

        IInMemoryBus InMemory { get; }
    }
}
