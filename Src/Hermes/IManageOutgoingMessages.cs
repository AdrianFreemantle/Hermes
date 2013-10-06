using System.Collections.Generic;

using Hermes.Messaging;

namespace Hermes
{
    public interface IManageOutgoingMessages 
    {
        void Add(OutgoingMessage outgoingMessage);
        void Add(IEnumerable<OutgoingMessage> outgoingMessages);
        void Send();
    }
}