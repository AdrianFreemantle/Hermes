using System;
using System.Reflection;

namespace Hermes.Reflector
{
    public class MessageOriginator
    {
        public Type OriginatorType { get; set; }
        public MethodInfo BusMethod { get; set; }
        public MethodInfo OriginatorMethod { get; set; }
        public Type MessageType { get; set; }

        public MessageOriginator(Type originatorType, MethodInfo originatorMethod, MethodInfo busMethod, Type messageType)
        {
            OriginatorType = originatorType;
            OriginatorMethod = originatorMethod;
            BusMethod = busMethod;
            MessageType = messageType;
        }
    }
}