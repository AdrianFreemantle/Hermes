using System;
using System.Collections.Generic;

namespace EventStore
{
    public class OptimisticEventStore : ICommitEvents, IStoreEvents
    {
        private readonly IPersistStreams persistence;

        public OptimisticEventStore(IPersistStreams persistence)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException("persistence");
            }

            this.persistence = persistence;
        }

        public virtual IEnumerable<ICommit> GetFrom(string bucketId, string streamId, int minRevision, int maxRevision)
        {
            return persistence.GetFrom(bucketId, streamId, minRevision, maxRevision);
        }

        public virtual ICommit Commit(CommitAttempt attempt)
        {
            ICommit commit = persistence.Commit(attempt);
            return commit;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual IEventStream CreateStream(string bucketId, string streamId)
        {
            return new OptimisticEventStream(bucketId, streamId, this);
        }

        public virtual IEventStream OpenStream(string bucketId, string streamId, int minRevision, int maxRevision)
        {
            maxRevision = maxRevision <= 0 ? int.MaxValue : maxRevision;
            return new OptimisticEventStream(bucketId, streamId, this, minRevision, maxRevision);
        }

        public void StartDispatchScheduler()
        {
        }

        public virtual IPersistStreams Advanced
        {
            get { return persistence; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            persistence.Dispose();
        }
    }
}