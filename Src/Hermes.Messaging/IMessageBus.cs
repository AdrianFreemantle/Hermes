using System;

namespace Hermes.Messaging
{
    public interface IMessageBus  
    {
        ICallback Send(params ICommand[] messages);
        ICallback Send(Address address, params ICommand[] messages);
        ICallback Send(Address address, Guid corrolationId, params ICommand[] messages);
        ICallback Send(Address address, Guid corrolationId, TimeSpan timeToLive, params ICommand[] messages);
        ICallback Send(Guid corrolationId, params ICommand[] messages);
        ICallback Send(Guid corrolationId, TimeSpan timeToLive, params ICommand[] messages);

        void Publish(params IEvent[] messages);
        void Publish(Guid correlationId, params IEvent[] messages);

        void Reply(params IMessage[] messages);
        void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible;

        void Defer(TimeSpan delay, params ICommand[] messages);
        void Defer(TimeSpan delay, Guid corrolationId, params ICommand[] messages);

        IMessageContext CurrentMessageContext { get; }
    }
}
