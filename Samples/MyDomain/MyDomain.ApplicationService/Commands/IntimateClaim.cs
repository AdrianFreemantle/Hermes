using System;
using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService.Commands
{
    public class IntimateClaim : Command
    {
        public string ClaimEventId { get; set; }
    }
}