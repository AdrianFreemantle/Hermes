using System;
using System.Collections.Generic;

namespace Hermes.Reflector
{
    public interface IContractWriter
    {
        void WriteDetails(MessageType messageType, ICollection<Type> contractTypes, ICollection<HandlerDetail> handlers, ICollection<MessageOriginator> originators);
    }
}