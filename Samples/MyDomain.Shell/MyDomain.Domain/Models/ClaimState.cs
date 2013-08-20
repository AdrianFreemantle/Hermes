using System;
using MyDomain.Domain.Events;

namespace MyDomain.Domain.Models
{
    public class ClaimState
    {
        public Guid Id { get; private set; }
        public decimal Amount { get; private set; }
        public Guid ClaimEventId { get; private set; }

        public void When(ClaimRegistered @event)
        {
            Id = @event.ClaimId;
            Amount = @event.Amount;
            ClaimEventId = @event.ClaimEventId;
        }
    }
}