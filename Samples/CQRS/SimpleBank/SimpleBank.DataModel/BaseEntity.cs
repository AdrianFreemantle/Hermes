using System;

using Hermes.EntityFramework;

namespace SimpleBank.DataModel
{
    public abstract class BaseEntity : IPersistenceAudit 
    {
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedTimestamp { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}
