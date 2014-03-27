using System;
using System.Collections.Generic;

namespace IntegrationTest.Client.Contracts
{
    public class RecordAddedToDatabase : IRecordAddedToDatabase_V2
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