using System;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Core.Deferment
{
    public class InMemoryTimeoutPersistence : IPersistTimeouts
    {
        readonly IList<TimeoutData> storage = new List<TimeoutData>();
        readonly object lockObject = new object();

        public IEnumerable<Guid> GetExpired()
        {
            lock (lockObject)
            {
                var results = storage
                    .Where(data => data.Expires <= DateTime.UtcNow)
                    .OrderBy(data => data.Expires)
                    .Select(t => t.Id)
                    .ToList();
                
                return results;
            }
        }

        public void Add(TimeoutData timeout)
        {
            lock (lockObject)
            {
                timeout.Id = Guid.NewGuid();
                storage.Add(timeout);
            }
        }

        public bool TryRemove(Guid timeoutId, out TimeoutData timeoutData)
        {
            lock (lockObject)
            {
                timeoutData = storage.SingleOrDefault(t => t.Id == timeoutId);
                return timeoutData != null && storage.Remove(timeoutData);
            }
        }
    }
}