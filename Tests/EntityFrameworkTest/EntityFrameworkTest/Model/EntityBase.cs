using System;
using Hermes.Persistence;

namespace EntityFrameworkTest.Model
{
    public abstract class EntityBase : IPersistenceAudit, ISequentialGuidId
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime ModifiedTimestamp { get; set; }
        public virtual DateTime CreatedTimestamp { get; set; }
        public virtual string ModifiedBy { get; set; }
        public virtual string CreatedBy { get; set; }
    }
}