using System;

namespace Hermes.Messaging
{
    public interface IManageSubscriptions
    {
        void Subscribe<T>();
        void Subscribe(Type messageType);
        void Unsubscribe<T>();
        void Unsubscribe(Type messageType);
    }
}