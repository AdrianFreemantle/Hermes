using System.Collections.Generic;

namespace Hermes
{
    public interface IProcessMessages
    {
        void Process(MessageEnvelope envelope);
        void DispatchToHandlers(IEnumerable<object> messageBodies);
    }
}
