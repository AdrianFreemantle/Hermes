﻿using System;

namespace Hermes.Messaging.Bus
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
            subscriptionStore.Subscribe(Address.Local, messageType);
        }

        public void Unsubscribe<T>()
        {
            Unsubscribe(typeof(T));
        }

        public void Unsubscribe(Type messageType)
        {
            subscriptionStore.Unsubscribe(Address.Local, messageType);
        }
    }
}