using System;
using MyDomain.Domain.Events;

namespace MyDomain.Domain.Models
{
    public class ClaimEvent : Aggregate
    {
        private readonly ClaimEventState state;

        protected ClaimEvent(Guid id)
        {
            Id = id;
            state = new ClaimEventState();
        }

        public static ClaimEvent Intimate(Guid id)
        {
            var claimEvent = new ClaimEvent(id);

            claimEvent.RaiseEvent(new ClaimEventIntimated { Id = id });
            return claimEvent;
        }

        public Claim RegisterClaim(decimal amount)
        {
            return new Claim(amount, Id);
        }

        protected override void ApplyEvent(object @event)
        {
            state.When((dynamic)@event);
        }
    }
}