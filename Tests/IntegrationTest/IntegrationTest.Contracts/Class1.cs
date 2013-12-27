using System;
using System.Collections.Generic;
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
        public int RecordNumber { get; private set; }
        public List<Guid> RandomData { get; private set; }

        public AddRecordToDatabase(int recordNumber)
        {
            RecordNumber = recordNumber;
            RecordId = SequentialGuid.New();
            RandomData = new List<Guid>();

            for (int i = 0; i < 10; i++)
            {
                RandomData.Add(Guid.NewGuid());
            }
        }
    }

    public class RecordAddedToDatabase : IEvent
    {
        public Guid RecordId { get; private set; }
        public List<Guid> RandomData { get; private set; }

        public RecordAddedToDatabase(Guid recordId)
        {
            RecordId = recordId;

            RandomData = new List<Guid>();

            for (int i = 0; i < 10; i++)
            {
                RandomData.Add(Guid.NewGuid());
            }
        }
    }
}
