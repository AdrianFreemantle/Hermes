using System;

namespace MyDomain.Domain.Events
{
    public class ClaimEventIntimated : DomainEvent
    {
        public DateTime IntimatedTime { get; set; }

        public ClaimEventIntimated()
        {
            IntimatedTime = DateTime.Now;
        }
    }
}