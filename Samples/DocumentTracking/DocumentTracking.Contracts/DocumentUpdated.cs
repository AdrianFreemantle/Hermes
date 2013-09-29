using System;

using Clientele.Core.Messaging;

namespace DocumentTracking.Contracts
{
    public class DocumentUpdated : Event
    {
        public Guid DocumentId { get; set; }
        public string IfaNumber { get; set; }
        public string PolicyNumber { get; set; }
    }
}
