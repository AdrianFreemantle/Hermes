using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SubscriptionCache
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private readonly Dictionary<string, SubscriptionCacheItem> subscriptionCache;
        
        public SubscriptionCache()
        {
            subscriptionCache = new Dictionary<string, SubscriptionCacheItem>();
        }

        public bool TryGetSubscribers(IEnumerable<Type> contracts, out IEnumerable<Address> subscribers)
        {
            locker.EnterReadLock();
            subscribers = null;

            var allSubscriptions = new List<Address>();
            
            try
            {
                foreach (var subscriptions in contracts.Select(GetSubscribers))
                {
                    if (subscriptions == null)
                        return false;

                    allSubscriptions.AddRange(subscriptions);
                }

                subscribers = allSubscriptions;
                return true;
            }
            finally 
            {
                locker.ExitReadLock(); 
            }
        }

        private IEnumerable<Address> GetSubscribers(Type contractType)
        {
            if (!subscriptionCache.ContainsKey(contractType.FullName))
            {
                return null;
            }

            if (subscriptionCache[contractType.FullName].HasExpired)
            {
                subscriptionCache.Remove(contractType.FullName);
                return null;
            }

            return subscriptionCache[contractType.FullName].Subscribers;
        }

        public void UpdateSubscribers(Type contractType, IEnumerable<Address> subscribers, TimeSpan cacheValidityPeriod)
        {
            locker.EnterWriteLock();

            try
            {
                subscriptionCache[contractType.FullName] = new SubscriptionCacheItem(subscribers, cacheValidityPeriod);
            }
            finally 
            {
                locker.ExitWriteLock();
            }
        }
    }
}