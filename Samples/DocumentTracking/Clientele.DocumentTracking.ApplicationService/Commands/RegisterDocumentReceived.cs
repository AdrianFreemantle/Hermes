using System;

namespace Clientele.DocumentTracking.ApplicationService.Commands
{
    public class RegisterDocumentReceived 
    {
        public Guid DocumentId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Source { get; set; }
        public string User { get; set; }
    }
}