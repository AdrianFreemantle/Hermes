using System;
using System.Collections.Generic;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    public interface ITransportMessageFactory
    {
        TransportMessage BuildTransportMessage(object[] messages);
        TransportMessage BuildTransportMessage(Guid correlationId, object[] messages);
        TransportMessage BuildTransportMessage(Guid correlationId, TimeSpan timeToLive, object[] messages);
        TransportMessage BuildTransportMessage(Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers);
        TransportMessage BuildControlMessage(Guid correlationId, IEnumerable<HeaderValue> headers);
    }
}
