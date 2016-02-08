using System;

namespace Hermes.Messaging
{
    public interface IMessageBus  
    {
        ICallback Send<T>(T command) where T : class;
        ICallback Send<T>(Address address, T command) where T : class;
        ICallback Send<T>(Address address, Guid corrolationId, T command) where T : class;
        ICallback Send<T>(Address address, Guid corrolationId, TimeSpan timeToLive, T command) where T : class;
        ICallback Send<T>(Guid corrolationId, T command) where T : class;
        ICallback Send<T>(Guid corrolationId, TimeSpan timeToLive, T command) where T : class;

        void Publish<T>(T @event) where T : class;
        void Publish<T>(Guid correlationId, T @event) where T : class;

        void Reply<T>(T message) where T : class;
        void Reply<T>(Address address, Guid corrolationId, T message) where T : class;
        void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible;

        void Defer<T>(TimeSpan delay, T command) where T : class;
        void Defer<T>(TimeSpan delay, Guid corrolationId, T command) where T : class;

        IMessageContext CurrentMessage { get; }
    }
}
