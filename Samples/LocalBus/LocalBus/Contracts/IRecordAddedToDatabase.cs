using System;

namespace IntegrationTest.Client.Contracts
{
    public interface IRecordAddedToDatabase : IEvent
    {
        Guid RecordId { get; }
    }
}