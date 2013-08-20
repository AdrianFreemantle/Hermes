using System;

namespace MyDomain.Domain.Events
{
    public class ClaimEventIntimated 
    {
        public Guid Id { get; set; }
        public DateTime IntimatedTime { get; set; }

        public ClaimEventIntimated()
        {
            IntimatedTime = DateTime.Now;
        }
    }
}