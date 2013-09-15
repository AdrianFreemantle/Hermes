using System;
using MyDomain.Domain.Events;

namespace MyDomain.Domain.Models
{
    public class Claim : Aggregate
    {
        private readonly ClaimState state;

        protected Claim(Guid id)
        {
            Id = id;
            state = new ClaimState();
        }

        public Claim(Guid id, decimal amount, Guid claimEventId)
            : this(id)
        {
            RaiseEvent(new ClaimRegistered
            {
                ClaimId = Id,
                Amount = amount,
                ClaimEventId = claimEventId
            });
        }

        protected override void ApplyEvent(object @event)
        {
            (state).When((dynamic)@event);
        }
    }
}