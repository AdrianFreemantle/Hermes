using System.Collections.Generic;

namespace Hermes
{
    public interface IProcessMessages
    {
        void ProcessEnvelope(MessageEnvelope envelope);
        void ProcessMessages(IEnumerable<object> messageBodies);
    }
}
