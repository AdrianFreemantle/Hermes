using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface ITimeoutData
    {
        Guid MessageId { get; set; }
        string DestinationAddress { get; set; }
        byte[] Body { get; set; }
        DateTime Expires { get; set; }
        Guid CorrelationId { get; set; }
        IDictionary<string, string> Headers { get; set; }
    }
}