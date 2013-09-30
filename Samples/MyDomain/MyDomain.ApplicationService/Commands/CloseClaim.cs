using System;
using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService.Commands
{
    public class CloseClaim : Command
    {
        public Guid ClaimEventId { get; set; }
    }
}