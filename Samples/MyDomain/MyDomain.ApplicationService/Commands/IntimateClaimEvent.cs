using System;
using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService.Commands
{
    public class IntimateClaimEvent : Command
    {
        public Guid ClaimEventId { get; set; }
    }
}