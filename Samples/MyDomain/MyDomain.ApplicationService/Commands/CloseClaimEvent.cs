using System;
using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService.Commands
{
    public class CloseClaimEvent : Command
    {
        public Guid ClaimEventId { get; set; }
    }
}