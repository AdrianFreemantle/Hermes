using System;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SubscriptionCacheItem
    {
        private readonly DateTime expires;
        public bool HasExpired { get { return DateTime.Now >= expires; } }
        public IReadOnlyCollection<Address> Subscribers { get; private set; }

        public SubscriptionCacheItem(IEnumerable<Address> subscribers, TimeSpan cacheValidityPeriod)
        {
            expires = DateTime.Now.Add(cacheValidityPeriod);
            Subscribers = subscribers.ToArray();
        }
    }
}