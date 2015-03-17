using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Logging;
using Hermes.Persistence;

namespace EventStore
{
    public sealed class OptimisticEventStream : IEventStream
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(OptimisticEventStream));
        private readonly ICollection<EventMessage> committed = new List<EventMessage>();
        private readonly IDictionary<string, object> committedHeaders = new Dictionary<string, object>();
        private readonly ICollection<EventMessage> events = new LinkedList<EventMessage>();
        private readonly ICollection<Guid> identifiers = new HashSet<Guid>();
        private readonly ICommitEvents persistence;
        private readonly IDictionary<string, object> uncommittedHeaders = new Dictionary<string, object>();
        private bool disposed;

        public string BucketId { get; private set; }
        public string StreamId { get; private set; }
        public int StreamRevision { get; private set; }
        public int CommitSequence { get; private set; }

        public ICollection<EventMessage> CommittedEvents { get { return committed; } }
        public IDictionary<string, object> CommittedHeaders { get { return committedHeaders; } }
        public ICollection<EventMessage> UncommittedEvents { get { return events; } }
        public IDictionary<string, object> UncommittedHeaders { get { return uncommittedHeaders; } }

        public OptimisticEventStream(string bucketId, string streamId, ICommitEvents persistence)
        {
            BucketId = bucketId;
            StreamId = streamId;
            this.persistence = persistence;
        }

        public OptimisticEventStream(string bucketId, string streamId, ICommitEvents persistence, int minRevision, int maxRevision)
            : this(bucketId, streamId, persistence)
        {
            IEnumerable<ICommit> commits = persistence.GetFrom(bucketId, streamId, minRevision, maxRevision);
            PopulateStream(minRevision, maxRevision, commits);

            if (minRevision > 0 && committed.Count == 0)
            {
                throw new Exception("Stream not found");
            }
        }        
        
        public void Add(EventMessage uncommittedEvent)
        {
            if (uncommittedEvent == null || uncommittedEvent.Body == null)
            {
                return;
            }

            events.Add(uncommittedEvent);
        }

        public void CommitChanges(Guid commitId)
        {
            if (identifiers.Contains(commitId))
            {
                throw new Exception("Duplicate Commit Exception");
            }

            if (!HasChanges())
            {
                return;
            }
            
            PersistChanges(commitId);
        }

        public void ClearChanges()
        {
            events.Clear();
            uncommittedHeaders.Clear();
        }        

        private bool HasChanges()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Disposed");
            }

            if (events.Count > 0)
            {
                return true;
            }

            return false;
        }

        private void PersistChanges(Guid commitId)
        {
            CommitAttempt attempt = BuildCommitAttempt(commitId);

            ICommit commit = persistence.Commit(attempt);

            PopulateStream(StreamRevision + 1, attempt.StreamRevision, new[] { commit });
            ClearChanges();
        }

        private CommitAttempt BuildCommitAttempt(Guid commitId)
        {
            return new CommitAttempt(
                BucketId,
                StreamId,
                StreamRevision + events.Count,
                commitId,
                CommitSequence + 1,
                DateTime.UtcNow,
                uncommittedHeaders.ToDictionary(x => x.Key, x => x.Value),
                events.ToList());
        }

        private void PopulateStream(int minRevision, int maxRevision, IEnumerable<ICommit> commits)
        {
            foreach (var commit in commits ?? Enumerable.Empty<ICommit>())
            {
                identifiers.Add(commit.CommitId);

                CommitSequence = commit.CommitSequence;
                int currentRevision = commit.StreamRevision - commit.Events.Count + 1;

                if (currentRevision > maxRevision)
                    return;

                CopyToCommittedHeaders(commit);
                CopyToEvents(minRevision, maxRevision, currentRevision, commit);
            }
        }

        private void CopyToCommittedHeaders(ICommit commit)
        {
            foreach (var key in commit.Headers.Keys)
            {
                committedHeaders[key] = commit.Headers[key];
            }
        }

        private void CopyToEvents(int minRevision, int maxRevision, int currentRevision, ICommit commit)
        {
            foreach (var @event in commit.Events)
            {
                if (currentRevision > maxRevision)
                {
                    break;
                }

                if (currentRevision++ < minRevision)
                {
                    continue;
                }

                committed.Add(@event);
                StreamRevision = currentRevision - 1;
            }
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}
