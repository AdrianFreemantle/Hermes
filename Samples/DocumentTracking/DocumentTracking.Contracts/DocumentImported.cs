using System;

using Clientele.Core.Messaging;

namespace DocumentTracking.Contracts
{
    public class DocumentImported : Event
    {
        public Guid DocumentId { get; set; }  
    }
}