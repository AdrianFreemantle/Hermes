using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Messages;

namespace Hermes.Transports
{
    public interface IProcessMessage
    {
        bool Process(MessageEnvelope envelope);
    }

    public interface IDispatchMessagesToHandlers
    {
        void DispatchToHandlers(object message);
        void DispatchToHandlers(IEnumerable<object> messages);
    }
}
