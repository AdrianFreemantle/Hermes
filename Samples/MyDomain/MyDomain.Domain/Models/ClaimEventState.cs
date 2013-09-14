using System;
using MyDomain.Domain;
using MyDomain.Domain.Events;

namespace MyDomain.Domain.Models
{
    public class ClaimEventState
    {
        public DateTime IntimatedDate { get; private set; }

        public void When(ClaimEventIntimated @event)
        {
            IntimatedDate = @event.IntimatedTime;
        }
    }
}