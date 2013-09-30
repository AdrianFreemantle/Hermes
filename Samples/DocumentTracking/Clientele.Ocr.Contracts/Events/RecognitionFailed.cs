using System;
using Clientele.Core.Messaging;

namespace Clientele.Ocr.Contracts.Events
{
    public class RecognitionFailed : Event
    {
        public Guid DocumentId { get; set; }
        public string ErrorMessage { get; set; }
    }
}