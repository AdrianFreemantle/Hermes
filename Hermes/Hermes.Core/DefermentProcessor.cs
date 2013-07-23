using System;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Core
{
    public class DefermentProcessor : IProcessMessages
    {
        public void Process(MessageEnvelope envelope)
        {
            
        }
    }

    public interface IPersistTimeouts
    {
        IEnumerable<Guid> GetNext(DateTime startDate);
        void Add(TimeoutData timeout);
        bool TryRemove(Guid timeoutId, out TimeoutData timeoutData);
    }

    public class InMemoryTimeoutPersistence : IPersistTimeouts
    {
        readonly IList<TimeoutData> storage = new List<TimeoutData>();
        readonly object lockObject = new object();

        public IEnumerable<Guid> GetNext(DateTime startDate)
        {
            lock (lockObject)
            {
                var results = storage
                    .Where(data => data.ExpiryTime > startDate && data.ExpiryTime <= DateTime.UtcNow)
                    .OrderBy(data => data.ExpiryTime)
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

    public class TimeoutData 
    {
        public Guid Id { get; set; }
        public Address Destination { get; set; }
        public byte[] State { get; set; }
        public DateTime ExpiryTime { get; set; }
        public Guid CorrelationId { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public override string ToString()
        {
            return string.Format("Timeout({0}) - Expires:{1}", Id, ExpiryTime);
        }

        public MessageEnvelope ToMessageEnvelope()
        {
            return null;
        }

        /// <summary>
        /// Original ReplyTo address header.
        /// </summary>
        public const string OriginalReplyToAddress = "Hermes.Timeout.ReplyToAddress";
    }
}