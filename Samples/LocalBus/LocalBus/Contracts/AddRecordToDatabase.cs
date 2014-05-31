using System;
using System.Collections.Generic;
using Contracts;
using Hermes;

namespace LocalBus.Contracts
{
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
}