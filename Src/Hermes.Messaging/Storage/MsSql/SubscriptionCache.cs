using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SubscriptionCache
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private readonly Dictionary<string, SubscriptionCacheItem> subscriptionCache;
        private readonly TimeSpan monitoringPeriod = TimeSpan.FromSeconds(1);
        private readonly Timer timer;
        
        public SubscriptionCache()
        {
            subscriptionCache = new Dictionary<string, SubscriptionCacheItem>();

            timer = new Timer
            {
                Interval = monitoringPeriod.TotalMilliseconds,
                AutoReset = false,
            };

            timer.Elapsed += Elapsed;
            timer.Start();
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

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            locker.EnterWriteLock();

            try
            {
                var keys = subscriptionCache.Keys.ToArray();

                foreach (var key in keys)
                {
                    RemoveCacheItemIfExpired(key);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        private void RemoveCacheItemIfExpired(string key)
        {
            if (subscriptionCache[key].HasExpired)
            {
                subscriptionCache.Remove(key);
            }
        }
    }
}