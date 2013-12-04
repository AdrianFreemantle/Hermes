using System;

using Hermes;

namespace IntegrationTest.Contracts
{
    public interface ICommand
    {
    }

    public interface IEvent
    {
    }

    public class AddRecordToDatabase : ICommand
    {
        public Guid RecordId { get; private set; }

        public AddRecordToDatabase()
        {
            RecordId = SequentialGuid.New();
        }
    }

    public class RecordAddedToDatabase : IEvent
    {
        public Guid RecordId { get; private set; }

        public RecordAddedToDatabase(Guid recordId)
        {
            RecordId = recordId;
        }
    }
}
