using System;
using Clientele.Core.Messaging;

namespace Clientele.Ocr.Contracts.Events
{
    public class ExportFailed : Event
    {
        public Guid DocumentId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
