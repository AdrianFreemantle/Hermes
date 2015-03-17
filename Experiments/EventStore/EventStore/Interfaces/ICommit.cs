using System;
using System.Collections.Generic;

namespace EventStore
{
    public interface ICommit
    {
        string BucketId { get; }
        string StreamId { get; }
        int StreamRevision { get; }
        Guid CommitId { get; }
        int CommitSequence { get; }
        DateTime CommitStamp { get; }
        IDictionary<string, object> Headers { get; }
        ICollection<EventMessage> Events { get; }
        Guid Id { get; }
    }
}