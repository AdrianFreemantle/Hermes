using System;

namespace MyDomain.Persistence.ReadModel.Models
{
    public class Claim
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }

        public Guid ClaimEventId { get; set; }
        public virtual ClaimEvent ClaimEvent { get; set; }
    }
}