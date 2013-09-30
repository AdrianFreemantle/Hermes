using System;
using System.Collections.Generic;
using Clientele.Core.Messaging;

namespace Clientele.Ocr.Contracts.Events
{
    public class VerificationDone : Event
    {
        public Guid DocumentId { get; set; }
        public Dictionary<string, string> MetaData { get; set; } 
        public string VerfiedByUser { get; set; }
    }
}