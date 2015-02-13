﻿using System;

namespace EntityFrameworkTest.Model
{
    public class Employee : EntityBase
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }

        public virtual Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}