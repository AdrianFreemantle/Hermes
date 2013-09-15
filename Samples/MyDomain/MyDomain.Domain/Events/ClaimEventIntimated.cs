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

    public class ChangedIntimatedDate
    {
        public Guid Id { get; set; }
        public DateTime IntimatedTime { get; set; }

        public ChangedIntimatedDate()
        {
            IntimatedTime = DateTime.Now;
        }
    }
}