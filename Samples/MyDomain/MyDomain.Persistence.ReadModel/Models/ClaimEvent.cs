using System;

namespace MyDomain.Persistence.ReadModel.Models
{
    public class ClaimEvent
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}