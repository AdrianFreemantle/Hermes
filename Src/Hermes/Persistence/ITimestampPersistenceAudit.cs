using System;

namespace Hermes.Persistence
{
    public interface ITimestampPersistenceAudit
    {
        DateTime ModifiedTimestamp { get; set; }
        DateTime CreatedTimestamp { get; set; }
    }
}