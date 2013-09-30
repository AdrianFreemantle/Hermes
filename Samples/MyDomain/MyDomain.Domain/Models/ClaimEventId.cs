using System;

namespace MyDomain.Domain.Models
{
    public class ClaimEventId : Identity
    {
        public ClaimEventId(string id) 
            : base(id)
        {
        }
    }
}