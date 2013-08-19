using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Hermes
{
    public interface IProcessMessages
    {
        void Process(MessageEnvelope envelope);
    }
}
