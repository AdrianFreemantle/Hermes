using System;

namespace Hermes.Caching
{
    internal class CachedObject : IDisposable
    {
        private readonly TimeSpan cacheValidityPeriod;
        private readonly CacheTimer cacheTimer;
        private readonly object cachedObject;
        private bool disposed;
        private DateTime expires;

        public bool HasExpired { get { return DateTime.Now >= expires; } }

        public CachedObject(object cachedObject, TimeSpan cacheValidityPeriod, CacheTimer cacheTimer)
        {
            this.cachedObject = cachedObject;
            this.cacheValidityPeriod = cacheValidityPeriod;
            this.cacheTimer = cacheTimer;
            SetExpiryTime();
        }

        public T GetCachedObject<T>()
        {
            ResetExpiryTime();

            return (T)cachedObject;
        }

        private void ResetExpiryTime()
        {
            if (cacheTimer == CacheTimer.Sliding)
            {
                SetExpiryTime();
            }
        }

        private void SetExpiryTime()
        {
            expires = DateTime.Now.Add(cacheValidityPeriod);
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
                if (cachedObject is IDisposable)
                {
                    ((IDisposable)cachedObject).Dispose();
                }
            }

            disposed = true;
        }
    }
}