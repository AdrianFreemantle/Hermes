using System;
using System.Collections.Generic;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Timeouts
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