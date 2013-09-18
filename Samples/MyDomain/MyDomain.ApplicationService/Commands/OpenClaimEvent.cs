using System;
using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService.Commands
{
    public class OpenClaimEvent : Command
    {
        public Guid ClaimEventId { get; set; }
    }
}