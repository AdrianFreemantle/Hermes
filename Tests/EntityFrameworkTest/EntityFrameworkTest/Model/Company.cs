using System.Collections.Generic;

namespace EntityFrameworkTest.Model
{
    public class Company : EntityBase
    {
        public virtual string Name { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }
}