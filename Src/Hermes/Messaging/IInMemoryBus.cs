using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IInMemoryBus
    {
        void Raise(object @event);
        void Raise(ICollection<object> events);
        void Execute(object command);
        void Execute(ICollection<object> commands);
    }
}