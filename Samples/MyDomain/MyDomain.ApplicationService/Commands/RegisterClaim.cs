using System;

namespace MyDomain.ApplicationService
{
    public class RegisterClaim
    {
        public Guid ClaimEventId { get; set; }
        public Guid ClaimId { get; set; }
        public decimal Amount { get; set; }
    }
}