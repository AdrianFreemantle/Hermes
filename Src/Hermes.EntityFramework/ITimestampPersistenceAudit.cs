using System;

namespace Hermes.EntityFramework
{
    public interface ITimestampPersistenceAudit
    {
        DateTime ModifiedTimestamp { get; set; }
        DateTime CreatedTimestamp { get; set; }
    }
}