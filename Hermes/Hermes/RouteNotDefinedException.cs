using System;

namespace Hermes
{
    public class RouteNotDefinedException : Exception
    {
        public Type MessageType { get; private set; }

        public RouteNotDefinedException(Type messageType, string message)
            : base(message)
        {
            MessageType = messageType;
        }

        public RouteNotDefinedException(Type messageType)
            : base(DefaultMessage(messageType))
        {
            MessageType = messageType;
        }

        private static string DefaultMessage(Type messageType)
        {
            return String.Format("No routes have been registered for message type {0}", messageType.FullName);
        }
    }
}