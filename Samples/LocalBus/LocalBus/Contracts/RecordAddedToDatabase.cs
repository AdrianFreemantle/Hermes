using System;
using System.Collections.Generic;
using Contracts;

namespace LocalBus.Contracts
{
    public class RecordAddedToDatabase : IRecordAddedToDatabase_V2
    {
        public Guid RecordId { get; private set; }
        public List<Guid> RandomData { get; private set; }
        public int RecordNumber { get; private set; }

        public RecordAddedToDatabase(Guid recordId, int recordNumber)
        {
            RecordId = recordId;
            RecordNumber = recordNumber;

            RandomData = new List<Guid>();

            for (int i = 0; i < 10; i++)
            {
                RandomData.Add(Guid.NewGuid());
            }
        }
    }
}