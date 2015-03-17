using System.Collections.Generic;

namespace EventStore
{
    public interface ICommitEvents
    {
        IEnumerable<ICommit> GetFrom(string bucketId, string streamId, int minRevision, int maxRevision);
        ICommit Commit(CommitAttempt attempt);
    }
}