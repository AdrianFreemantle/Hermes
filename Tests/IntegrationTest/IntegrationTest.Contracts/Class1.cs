using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Hermes;
using Hermes.Messaging;

namespace IntegrationTest.Contracts
{
    public interface ICommand
    {
    }

    public interface IEvent
    {
    }

    [DataContract]
    public class AddRecordToDatabase : ICommand
    {
        [DataMember]
        public Guid RecordId { get; private set; }

        [DataMember]
        public int RecordNumber { get; private set; }
        
        [DataMember]
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

    public interface IRecordAddedToDatabase : IEvent
    {
        Guid RecordId { get; }
    }

    public interface IRecordAddedToDatabase_V2 : IRecordAddedToDatabase
    {
        List<Guid> RandomData { get; }
    }
}
