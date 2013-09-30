using System;
using MyDomain.Domain.Events;

namespace MyDomain.Domain.Models
{
    public class ClaimEvent : RestorableTypedAggregate<ClaimEventState>
    {
        protected ClaimEvent(string id) 
            : base(new ClaimEventId(id))
        {
        }

        public static ClaimEvent Intimate(string id)
        {
            var claimEvent = new ClaimEvent(id);
            claimEvent.RaiseEvent(new ClaimEventIntimated());
            return claimEvent;
        }

        public void Open()
        {
            RaiseEvent(new ClaimEventOpened());
        }

        public void Close()
        {
            RaiseEvent(new ClaimEventClosed());
        }
    }
}