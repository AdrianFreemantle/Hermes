using System;
using Clientele.Core.Messaging;

namespace Clientele.Ocr.Contracts.Events
{
    public class RecognitionDone : Event
    {
        public Guid DocumentId { get; set; }
    }
}