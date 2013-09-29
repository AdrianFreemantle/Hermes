using System;

using Clientele.Core.Messaging;

namespace DocumentTracking.Contracts
{
    public class DocumentReceived : Event
    {
        public Guid DocumentId { get; set; }
        public string Source { get; set; }
    }
}