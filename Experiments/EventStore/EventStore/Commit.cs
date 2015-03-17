using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace EventStore
{
    public class Commit : ICommit
    {
        private readonly string bucketId;
        private readonly string streamId;
        private readonly int streamRevision;
        private readonly Guid commitId;
        private readonly int commitSequence;
        private readonly DateTime commitStamp;
        private readonly IDictionary<string, object> headers;
        private readonly ICollection<EventMessage> events;
        private readonly Guid id;

        public Commit(
            Guid id,
            string bucketId,
            string streamId,
            int streamRevision,
            Guid commitId,
            int commitSequence,
            DateTime commitStamp,
            IDictionary<string, object> headers,
            IEnumerable<EventMessage> events)
        {
            this.bucketId = bucketId;
            this.streamId = streamId;
            this.streamRevision = streamRevision;
            this.commitId = commitId;
            this.commitSequence = commitSequence;
            this.commitStamp = commitStamp;
            this.id = id;
            this.headers = headers ?? new Dictionary<string, object>();
            this.events = events == null ?
                new ReadOnlyCollection<EventMessage>(new List<EventMessage>()) :
                new ReadOnlyCollection<EventMessage>(new List<EventMessage>(events));
        }

        public string BucketId
        {
            get { return bucketId; }
        }

        public string StreamId
        {
            get { return streamId; }
        }

        public int StreamRevision
        {
            get { return streamRevision; }
        }

        public Guid CommitId
        {
            get { return commitId; }
        }

        public int CommitSequence
        {
            get { return commitSequence; }
        }

        public DateTime CommitStamp
        {
            get { return commitStamp; }
        }

        public IDictionary<string, object> Headers
        {
            get { return headers; }
        }

        public ICollection<EventMessage> Events
        {
            get
            {
                return events;
            }
        }

        public Guid Id
        {
            get
            {
                return id;
            }
        }
    }
}