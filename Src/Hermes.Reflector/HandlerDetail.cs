using System;
using System.Collections.Generic;

namespace Hermes.Reflector
{
    public class HandlerDetail
    {
        public HashSet<Type> HandledContractTypes { get; private set; }
        public Type HandlerType { get; private set; }

        public HandlerDetail(Type handlerType, IEnumerable<Type> handledTypes)
        {
            HandlerType = handlerType;
            HandledContractTypes = new HashSet<Type>(handledTypes);
        }
    }
}