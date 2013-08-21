using System;
using Hermes.Configuration;
using Hermes.Messaging;
using Hermes.Subscriptions;

namespace Hermes.Core
{
    public class SubscriptionManager : IManageSubscriptions
    {
        private readonly IStoreSubscriptions subscriptionStore;

        public SubscriptionManager(IStoreSubscriptions subscriptionStore)
        {
            this.subscriptionStore = subscriptionStore;
        }

        public void Subscribe<T>()
        {
            Subscribe(typeof(T));
        }

        public void Subscribe(Type messageType)
        {
            subscriptionStore.Subscribe(Settings.ThisEndpoint, messageType);
        }

        public void Unsubscribe<T>()
        {
            Unsubscribe(typeof(T));
        }

        public void Unsubscribe(Type messageType)
        {
            subscriptionStore.Unsubscribe(Settings.ThisEndpoint, messageType);
        }
    }
}