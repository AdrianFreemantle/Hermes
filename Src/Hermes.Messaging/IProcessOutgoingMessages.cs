using System.Collections.Generic;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    public interface IProcessOutgoingMessages
    {
        void Add(OutgoingMessage message);
        void Add(IEnumerable<OutgoingMessage> messages);
    }
}