using System;

namespace Hermes.Messaging
{
    public interface IMessageBus  
    {
        ICallback Send(params object[] messages);
        ICallback Send(Address address, params object[] messages);
        ICallback Send(Address address, Guid corrolationId, params object[] messages);
        ICallback Send(Address address, Guid corrolationId, TimeSpan timeToLive, params object[] messages);
        ICallback Send(Guid corrolationId, params object[] messages);
        ICallback Send(Guid corrolationId, TimeSpan timeToLive, params object[] messages);

        void Publish(params object[] messages);

        void Reply(params object[] messages);
        void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible;

        void Defer(TimeSpan delay, params object[] messages);
        void Defer(TimeSpan delay, Guid corrolationId, params object[] messages);

        IMessageContext CurrentMessageContext { get; }
    }
}
