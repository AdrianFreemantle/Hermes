using System;
using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService.Commands
{
    public class OpenClaim : Command
    {
        public Guid ClaimEventId { get; set; }
    }
}