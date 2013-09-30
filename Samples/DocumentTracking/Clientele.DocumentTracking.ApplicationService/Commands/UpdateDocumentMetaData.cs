using System;

namespace Clientele.DocumentTracking.ApplicationService.Commands
{
    public class UpdateDocumentMetaData 
    {
        public Guid DocumentId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string IfaNumber { get; set; }
        public string Source { get; set; }
        public string User { get; set; }
        public string PolicyNumber { get; set; }
    }
}
