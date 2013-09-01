using System;

namespace MyDomain.Domain.Events
{
    public class ClaimRegistered
    {
        public Guid ClaimId { get; set; }
        public Guid ClaimEventId { get; set; }
        public decimal Amount { get; set; }
    }
}