using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Hermes.Logging;
using Timer = System.Timers.Timer;

namespace Hermes.Messaging.Storage.MsSql
{
    public class SubscriptionCache
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (SubscriptionCache));

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

        public bool TryGetSubscribers(ICollection<Type> contracts, out ICollection<Address> subscribers)
        {
            Mandate.ParameterNotNull(contracts, "contracts");

            locker.EnterReadLock();
            subscribers = null;

            var allSubscriptions = new List<Address>();
            
            try
            {
                foreach (var subscriptions in contracts.Select(GetSubscribers))
                {
                    if (subscriptions == null)
                    {
                        return false;
                    }

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

        public void UpdateSubscribers(Type contractType, ICollection<Address> subscribers, TimeSpan cacheValidityPeriod)
        {
            if(!subscribers.Any())
                return;

            locker.EnterWriteLock();

            Logger.Info("Updating subsricption cache for event {0} with subscribers: {1}", contractType.FullName, String.Join(", ", subscribers));

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
            //bug this has been temporarily disabled while testing around integration events not being sent is concluded

            //if (subscriptionCache[key].HasExpired)
            //{
            //    Logger.Info("Clearing expired subscription cache for event {0}", key);
            //    subscriptionCache.Remove(key);
            //}
        }
    }
}