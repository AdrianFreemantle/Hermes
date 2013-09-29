using System;
using MyDomain.Domain;
using MyDomain.Domain.Events;

namespace MyDomain.Domain.Models
{
    public class ClaimEventState : IMemento
    {
        public IHaveIdentity Identity { get; set; }
        public DateTime IntimatedDate { get; private set; }
        public bool IsOpen { get; private set; }

        public void When(ClaimEventIntimated @event)
        {
            IntimatedDate = @event.IntimatedTime;
        }

        public void When(ClaimEventOpened @event)
        {
            IsOpen = true;
        }

        public void When(ClaimEventClosed @event)
        {
            IsOpen = false;
        }

    }
}