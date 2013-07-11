using System;

namespace Hermes
{
    public interface IMessageBus
    {
        void Send(params object[] messages);
        void Send(Address address, params object[] messages);

        void Publish(params object[] messages);

        void Subscribe<T>();
        void Subscribe(Type messageType);
        void Unsubscribe<T>();
        void Unsubscribe(Type messageType);
    }
}
