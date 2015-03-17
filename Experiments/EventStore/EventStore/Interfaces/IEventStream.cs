using System;
using System.Collections.Generic;

namespace EventStore
{
    public interface IEventStream : IDisposable
    {
        string BucketId { get; }
        string StreamId { get; }
        int StreamRevision { get; }
        int CommitSequence { get; }
        ICollection<EventMessage> CommittedEvents { get; }
        IDictionary<string, object> CommittedHeaders { get; }
        ICollection<EventMessage> UncommittedEvents { get; }
        IDictionary<string, object> UncommittedHeaders { get; }
        void Add(EventMessage uncommittedEvent);
        void CommitChanges(Guid commitId);
        void ClearChanges();
    }
}