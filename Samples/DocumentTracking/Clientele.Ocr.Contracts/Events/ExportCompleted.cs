using System;
using Clientele.Core.Messaging;

namespace Clientele.Ocr.Contracts.Events
{
    public class ExportCompleted : Event
    {
        public Guid DocumentId { get; set; }
        public Uri FilePath { get; set; }
    }
}