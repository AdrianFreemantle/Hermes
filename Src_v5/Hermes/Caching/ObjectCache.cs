using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Hermes.Caching
{
    public class ObjectCache : IDisposable
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private readonly Dictionary<Guid, CachedObject> cache;
        private readonly TimeSpan defaultCacheValidityPeriod;
        private readonly CacheTimer defaultCacheCacheTimer;
        private readonly Timer timer;
        private bool disposed;

        public ObjectCache()
            : this(TimeSpan.FromMinutes(1), TimeSpan.FromHours(1), CacheTimer.Absolute)
        {
        }

        public ObjectCache(TimeSpan cacheValidationInterval, TimeSpan defaultCacheValidityPeriod, CacheTimer defaultCacheCacheTimer)
        {
            cache = new Dictionary<Guid, CachedObject>();
            this.defaultCacheCacheTimer = defaultCacheCacheTimer;
            this.defaultCacheValidityPeriod = defaultCacheValidityPeriod;

            timer = new Timer
            {
                Interval = cacheValidationInterval.TotalMilliseconds,
                AutoReset = true,
            };

            timer.Elapsed += Elapsed;
            timer.Start();
        }

        public bool Contains(string key)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            
            Guid cacheKey = DeterministicGuid.Create(key);
            return Contains(cacheKey);
        }

        public bool Contains(Guid key)
        {
            Mandate.ParameterNotDefaut(key, "key");

            locker.EnterReadLock();

            try
            {
                return cache.ContainsKey(key);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public bool TryGetCachedObject<T>(string key, out T cachedItem)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");

            Guid cacheKey = DeterministicGuid.Create(key);

            return TryGetCachedObject(cacheKey, out cachedItem);
        }

        public bool TryGetCachedObject<T>(Guid key, out T cachedItem)
        {
            Mandate.ParameterNotDefaut(key, "key");

            locker.EnterReadLock();
            cachedItem = default(T);

            try
            {
                if (cache.ContainsKey(key))
                {
                    CachedObject cachedObject = cache[key];
                    cachedItem = cachedObject.GetCachedObject<T>();
                    return true;
                }
            }
            finally
            {
                locker.ExitReadLock();
            }

            return false;
        }

        public T GetOrUpdateCachedObject<T>(Guid key, Func<T> fetchUpdatedObject)
        {
            return GetOrUpdateCachedObject(key, fetchUpdatedObject, defaultCacheValidityPeriod, defaultCacheCacheTimer);
        }

        public T GetOrUpdateCachedObject<T>(string key, Func<T> fetchUpdatedObject)
        {
            return GetOrUpdateCachedObject(key, fetchUpdatedObject, defaultCacheValidityPeriod, defaultCacheCacheTimer);
        }

        public T GetOrUpdateCachedObject<T>(string key, Func<T> fetchUpdatedObject, TimeSpan cacheValidityPeriod, CacheTimer cacheTimer)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            Guid cacheKey = DeterministicGuid.Create(key);

            return GetOrUpdateCachedObject(cacheKey, fetchUpdatedObject, cacheValidityPeriod, cacheTimer);
        }

        public T GetOrUpdateCachedObject<T>(Guid key, Func<T> fetchUpdatedObject, TimeSpan cacheValidityPeriod, CacheTimer cacheTimer)
        {
            Mandate.ParameterNotDefaut(key, "key");
            Mandate.ParameterNotNull(fetchUpdatedObject, "fetchUpdatedObject");

            T cachedItem;

            if (TryGetCachedObject(key, out cachedItem))
                return cachedItem;

            cachedItem = fetchUpdatedObject();

            if (cachedItem.Equals(default(T)))
                throw new ObjectCacheException("Unable to retrieve an updated cache item");

            AddCachedObject(key, cachedItem, cacheValidityPeriod, cacheTimer);
            return cachedItem;
        }

        public void AddCachedObject(string key, object cacheItem)
        {
            AddCachedObject(key, cacheItem, defaultCacheValidityPeriod, defaultCacheCacheTimer);
        }

        public void AddCachedObject(Guid key, object cacheItem)
        {
            AddCachedObject(key, cacheItem, defaultCacheValidityPeriod, defaultCacheCacheTimer);
        }

        public void AddCachedObject(string key, object cacheItem, TimeSpan cacheValidityPeriod, CacheTimer cacheTimer)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");

            Guid cacheKey = DeterministicGuid.Create(key);

            AddCachedObject(cacheKey, cacheItem, cacheValidityPeriod, cacheTimer);
        }

        public void AddCachedObject(Guid key, object cacheItem, TimeSpan cacheValidityPeriod, CacheTimer cacheTimer)
        {
            Mandate.ParameterNotDefaut(key, "key");
            Mandate.ParameterNotNull(cacheItem, "cacheItem");

            locker.EnterWriteLock();

            try
            {
                cache[key] = new CachedObject(cacheItem, cacheValidityPeriod, cacheTimer);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void FlushCache()
        {
            locker.EnterWriteLock();

            try
            {
                foreach (var key in cache.Keys.ToArray())
                {
                    RemoveCacheItem(key);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void RemoveCachedObject(string key)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");

            Guid cacheKey = DeterministicGuid.Create(key);

            RemoveCachedObject(cacheKey);
        }

        public bool RemoveCachedObject(Guid key)
        {
            Mandate.ParameterNotDefaut(key, "key");
            locker.EnterWriteLock();

            try
            {
                if (cache.ContainsKey(key))
                {
                    return RemoveCacheItem(key);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }

            return false;
        }
        
        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            locker.EnterWriteLock();

            try
            {
                foreach (var key in cache.Keys.ToArray())
                {
                    RemoveCacheItemIfExpired(key);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        private void RemoveCacheItemIfExpired(Guid key)
        {
            if (cache[key].HasExpired)
            {
                RemoveCacheItem(key);
            }
        }

        private bool RemoveCacheItem(Guid key)
        {
            cache[key].Dispose();
            return cache.Remove(key);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                timer.Dispose();

                foreach (var cachedObject in cache.Values)
                {
                    cachedObject.Dispose();
                }
            }

            disposed = true;
        }
    }
}
