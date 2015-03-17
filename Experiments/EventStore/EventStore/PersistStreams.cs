using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EventStore.Persistence;
using Hermes;
using Hermes.Domain.Testing;
using Hermes.EntityFramework;
using Hermes.Serialization;

namespace EventStore
{
    public class PersistStreams : IPersistStreams
    {
        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1);

        private readonly IRepository<EventStream> eventStreams;
        private readonly ISerializeObjects serializer;
        private readonly Database database;

        public PersistStreams(EntityFrameworkUnitOfWork entityFrameworkUnitOfWork, ISerializeObjects serializer)
        {
            database = entityFrameworkUnitOfWork.GetDatabase();
            this.serializer = serializer;
            eventStreams = entityFrameworkUnitOfWork.GetRepository<EventStream>();
        }

        public bool IsDisposed { get; private set; }
        
        public void Initialize()
        {
        }

        public IEnumerable<ICommit> GetFrom(string bucketId, DateTime start)
        {
            Mandate.ParameterNotNullOrEmpty(bucketId, "bucketId");

            if (start < EpochTime)
                start = EpochTime;

            return eventStreams
                .Where(s => s.CreatedTimestamp >= start && s.BucketId == bucketId)
                .ToList()
                .ConvertAll(Converter());
        }

        public IEnumerable<ICommit> GetFromTo(string bucketId, DateTime start, DateTime end)
        {
            Mandate.ParameterNotNullOrEmpty(bucketId, "bucketId");

            if (start < EpochTime)
                start = EpochTime;

            if (end < EpochTime)
                end = EpochTime;

            return eventStreams
                .Where(s => s.CreatedTimestamp >= start && s.CreatedTimestamp <= end && s.BucketId == bucketId)
                .ToList()
                .ConvertAll(Converter());
        }

        public IEnumerable<ICommit> GetFrom(string checkpointToken)
        {
            LongCheckpoint checkpoint = LongCheckpoint.Parse(checkpointToken);

            return eventStreams
                .Where(s => s.CheckpointToken >= checkpoint.LongValue)
                .ToList()
                .ConvertAll(Converter());
        }

        public IEnumerable<ICommit> GetFrom(string bucketId, string streamId, int minRevision, int maxRevision)
        {
            Mandate.ParameterNotNullOrEmpty(bucketId, "bucketId");
            Mandate.ParameterNotNullOrEmpty(streamId, "streamId");

            var streamIdHash = ToHash(streamId);

            return eventStreams
                .Where(s => s.BucketId == bucketId && s.StreamIdHash == streamIdHash && s.StreamRevision <= minRevision && s.StreamRevision >= maxRevision)
                .ToList()
                .ConvertAll(Converter());
        }             

        public IEnumerable<ICommit> GetUndispatchedCommits()
        {
            return eventStreams
                .Where(s => s.Dispatched == false)
                .ToList()
                .ConvertAll(Converter());
        }

        public void MarkCommitAsDispatched(ICommit commit)
        {
            var item = eventStreams.GetOrCreate(s => s.ResourceId == commit.Id);

            item.Dispatched = true;
        }

        public void Purge()
        {
            database.ExecuteSqlCommand("TRUNCATE TABLE [EventStream]");
        }

        public void Purge(string bucketId)
        {
            var bucketIdParameter = new SqlParameter("@BucketId", bucketId);
            database.ExecuteSqlCommand("DELETE FROM TABLE [EventStream] WHERE BucketId = @BucketId", bucketIdParameter);
        }

        public void Drop()
        {
            Purge();
        }

        public void DeleteStream(string bucketId, string streamId)
        {
            var streamIdHash = ToHash(streamId);

            var bucketIdParameter = new SqlParameter("@BucketId", bucketId);
            var streamIdParameter = new SqlParameter("@StreamIdHash", streamIdHash);

            database.ExecuteSqlCommand("DELETE FROM TABLE [EventStream] WHERE BucketId = @BucketId AND StreamIdHash = StreamIdHash", bucketIdParameter, streamIdParameter);
        }

        public ICommit Commit(CommitAttempt attempt)
        {
            var headers = serializer.ToByteArray(attempt.Headers);
            var events = serializer.ToByteArray(attempt.Events);
            var streamIdHash = ToHash(attempt.StreamId);

            var eventStream = new EventStream
            {
                ResourceId = SequentialGuid.New(),
                BucketId = attempt.BucketId,
                StreamId = attempt.StreamId,
                StreamIdHash = streamIdHash,
                CommitId = attempt.CommitId,
                StreamRevision = attempt.StreamRevision,
                Items = attempt.Events.Count,
                CommitSequence = attempt.CommitSequence,
                Dispatched = false,
                Headers = headers,
                Payload = events,
            };

            eventStreams.Add(eventStream);

            return new Commit(eventStream.ResourceId, attempt.BucketId, attempt.StreamId, attempt.StreamRevision, attempt.CommitId, attempt.CommitSequence, attempt.CommitStamp, attempt.Headers, attempt.Events);
        }

        public ICheckpoint GetCheckpoint(string checkpointToken)
        {
            if (string.IsNullOrWhiteSpace(checkpointToken))
            {
                return new LongCheckpoint(-1);
            }

            return LongCheckpoint.Parse(checkpointToken);
        }   

        string ToHash(dynamic key)
        {
            byte[] hashBytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key.ToString()));
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        private Converter<EventStream, Commit> Converter()
        {
            return i =>
            {
                var headers = serializer.FromByteArray<Dictionary<string, object>>(i.Headers);
                var events = serializer.FromByteArray<List<EventMessage>>(i.Payload);

                return new Commit(i.ResourceId, i.BucketId, i.StreamId, i.StreamRevision, i.CommitId, i.CommitSequence, i.CreatedTimestamp, headers, events);
            };
        }

        public void Dispose()
        {
        }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        ///     Formats the string provided using the values specified.
        /// </summary>
        /// <param name="format">The string to be formated.</param>
        /// <param name="values">The values to be embedded into the string.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatWith(this string format, params object[] values)
        {
            return string.Format(CultureInfo.InvariantCulture, format ?? string.Empty, values);
        }
    }
}