using System;

namespace MyDomain.Domain.Models
{
    public class ClaimEventId : Identity
    {
        public ClaimEventId(Guid id) 
            : base(id)
        {
        }
    }
}