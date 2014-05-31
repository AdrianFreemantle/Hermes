using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface IRecordAddedToDatabase_V2 : IRecordAddedToDatabase
    {
        List<Guid> RandomData { get; }
        int RecordNumber { get; }
    }
}