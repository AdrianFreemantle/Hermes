using System;
using Hermes.Messages.Attributes;

namespace MyDomain.ApplicationService
{
    public class IntimateClaimEvent
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
    }
}