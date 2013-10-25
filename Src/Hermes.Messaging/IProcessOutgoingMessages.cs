using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IProcessOutgoingMessages 
    {
        void Add(OutgoingMessage outgoingMessage);
        void Add(IEnumerable<OutgoingMessage> outgoingMessages);
    }
}