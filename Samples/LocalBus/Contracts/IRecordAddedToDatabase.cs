using System;

namespace Contracts
{
    public interface IRecordAddedToDatabase : IEvent
    {
        Guid RecordId { get; }
    }
}