using System;
using System.Collections.Generic;

namespace IntegrationTest.Client.Contracts
{
    public interface IRecordAddedToDatabase_V2 : IRecordAddedToDatabase
    {
        List<Guid> RandomData { get; }
    }
}