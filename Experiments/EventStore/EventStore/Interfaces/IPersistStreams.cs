using System;
using System.Collections.Generic;

namespace EventStore
{
    public interface IPersistStreams : IDisposable
    {
        bool IsDisposed { get; }
        void Initialize();
        IEnumerable<ICommit> GetFrom(string bucketId, DateTime start);
        IEnumerable<ICommit> GetFrom(string checkpointToken = null);
        ICheckpoint GetCheckpoint(string checkpointToken = null);
        IEnumerable<ICommit> GetFromTo(string bucketId, DateTime start, DateTime end);
        IEnumerable<ICommit> GetUndispatchedCommits();
        void MarkCommitAsDispatched(ICommit commit);
        void Purge();
        void Purge(string bucketId);
        void Drop();
        void DeleteStream(string bucketId, string streamId);
        IEnumerable<ICommit> GetFrom(string bucketId, string streamId, int minRevision, int maxRevision);
        ICommit Commit(CommitAttempt attempt);
    }
}