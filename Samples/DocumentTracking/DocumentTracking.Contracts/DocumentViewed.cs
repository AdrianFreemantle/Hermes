using System;

using Clientele.Core.Messaging;

namespace DocumentTracking.Contracts
{
    public class DocumentViewed : Event
    {
        public Guid DocumentId { get; set; }
        public string UserName { get; set; }
    }
}