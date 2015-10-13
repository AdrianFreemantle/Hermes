using System;

namespace Hermes.Messaging
{
    public interface IMessageBus  
    {
        void Send(object command);
        void Send(Address address, object command);
        void Send(Address address, Guid corrolationId, object command);
        void Send(Address address, Guid corrolationId, TimeSpan timeToLive, object command);
        void Send(Guid corrolationId, object command);
        void Send(Guid corrolationId, TimeSpan timeToLive, object command);

        void Publish(object @event);
        void Publish(Guid correlationId, object @event);

        void Reply(object message);
        void Reply(Address address, Guid corrolationId, object message);
        void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible;

        void Defer(TimeSpan delay, object command);
        void Defer(TimeSpan delay, Guid corrolationId, object command);

        IMessageContext CurrentMessage { get; }
    }
}
