using System;
using Hermes.EntityFramework;
using Hermes.Persistence;

namespace EntityFrameworkTest.Model
{
    public class Employee : IPersistenceAudit, ISequentialGuidId
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }

        public virtual Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public virtual DateTime ModifiedTimestamp { get; set; }
        public virtual DateTime CreatedTimestamp { get; set; }
        public virtual string ModifiedBy { get; set; }
        public virtual string CreatedBy { get; set; }
    }
}