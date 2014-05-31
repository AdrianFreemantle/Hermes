using System;

namespace Hermes.Messaging
{
    public interface IMessageBus  
    {
        ICallback Send(object command);
        ICallback Send(Address address, object command);
        ICallback Send(Address address, Guid corrolationId, object command);
        ICallback Send(Address address, Guid corrolationId, TimeSpan timeToLive, object command);
        ICallback Send(Guid corrolationId, object command);
        ICallback Send(Guid corrolationId, TimeSpan timeToLive, object command);

        void Publish(object @event);
        void Publish(Guid correlationId, object @event);

        void Reply(object message);
        void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible;

        void Defer(TimeSpan delay, object command);
        void Defer(TimeSpan delay, Guid corrolationId, object command);

        IMessageContext CurrentMessage { get; }
    }
}
