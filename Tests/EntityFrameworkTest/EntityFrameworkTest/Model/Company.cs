using System;
using System.Collections.Generic;
using Hermes.EntityFramework;

namespace EntityFrameworkTest.Model
{
    public class Company : IPersistenceAudit, ISequentialGuidId
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }

        public virtual DateTime ModifiedTimestamp { get; set; }
        public virtual DateTime CreatedTimestamp { get; set; }
        public virtual string ModifiedBy { get; set; }
        public virtual string CreatedBy { get; set; }
    }
}