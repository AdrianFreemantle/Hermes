using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EventStore
{
    public class CommitAttempt
    {
        private readonly string bucketId;
        private readonly string streamId;
        private readonly int streamRevision;
        private readonly Guid commitId;
        private readonly int commitSequence;
        private readonly DateTime commitStamp;
        private readonly IDictionary<string, object> headers;
        private readonly ICollection<EventMessage> events;

        /// <summary>
        ///     Initializes a new instance of the Commit class.
        /// </summary>
        /// <param name="bucketId">The value which identifies bucket to which the the stream and the the commit belongs</param>
        /// <param name="streamId">The value which uniquely identifies the stream in a bucket to which the commit belongs.</param>
        /// <param name="streamRevision">The value which indicates the revision of the most recent event in the stream to which this commit applies.</param>
        /// <param name="commitId">The value which uniquely identifies the commit within the stream.</param>
        /// <param name="commitSequence">The value which indicates the sequence (or position) in the stream to which this commit applies.</param>
        /// <param name="commitStamp">The point in time at which the commit was persisted.</param>
        /// <param name="headers">The metadata which provides additional, unstructured information about this commit.</param>
        /// <param name="events">The collection of event messages to be committed as a single unit.</param>
        public CommitAttempt(
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
            this.headers = headers ?? new Dictionary<string, object>();
            this.events = events == null ?
                new ReadOnlyCollection<EventMessage>(new List<EventMessage>()) :
                new ReadOnlyCollection<EventMessage>(events.ToList());
        }

        /// <summary>
        ///     Gets the value which identifies bucket to which the the stream and the the commit belongs.
        /// </summary>
        public string BucketId
        {
            get { return bucketId; }
        }

        /// <summary>
        ///     Gets the value which uniquely identifies the stream to which the commit belongs.
        /// </summary>
        public string StreamId
        {
            get { return streamId; }
        }

        /// <summary>
        ///     Gets the value which indicates the revision of the most recent event in the stream to which this commit applies.
        /// </summary>
        public int StreamRevision
        {
            get { return streamRevision; }
        }

        /// <summary>
        ///     Gets the value which uniquely identifies the commit within the stream.
        /// </summary>
        public Guid CommitId
        {
            get { return commitId; }
        }

        /// <summary>
        ///     Gets the value which indicates the sequence (or position) in the stream to which this commit applies.
        /// </summary>
        public int CommitSequence
        {
            get { return commitSequence; }
        }

        /// <summary>
        ///     Gets the point in time at which the commit was persisted.
        /// </summary>
        public DateTime CommitStamp
        {
            get { return commitStamp; }
        }

        /// <summary>
        ///     Gets the metadata which provides additional, unstructured information about this commit.
        /// </summary>
        public IDictionary<string, object> Headers
        {
            get { return headers; }
        }

        /// <summary>
        ///     Gets the collection of event messages to be committed as a single unit.
        /// </summary>
        public ICollection<EventMessage> Events
        {
            get
            {
                return events;
            }
        }
    }
}