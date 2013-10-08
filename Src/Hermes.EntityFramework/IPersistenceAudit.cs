using System;

namespace Hermes.EntityFramework
{
    public interface IPersistenceAudit
    {
        string ModifiedBy { get; set; }
        string CreatedBy { get; set; }
        DateTime ModifiedTimestamp { get; set; }
        DateTime CreatedTimestamp { get; set; }
    }
}