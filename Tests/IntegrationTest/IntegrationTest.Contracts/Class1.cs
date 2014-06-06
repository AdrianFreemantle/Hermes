using System;
using System.Collections.Generic;

namespace IntegrationTest.Contracts
{
    public interface ICommand
    {
    }

    public interface IEvent
    {
    }

    public interface IRecordAddedToDatabase : IEvent
    {
        Guid RecordId { get; }
    }

    public interface IRecordAddedToDatabase_V2 : IRecordAddedToDatabase
    {
        List<Guid> RandomData { get; }
    }
}
