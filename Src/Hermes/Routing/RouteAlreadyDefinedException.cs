using System;

namespace Hermes.Routing
{
    public class RouteAlreadyDefinedException : Exception
    {
        public RouteAlreadyDefinedException(Type messageType)
            : base(GetMessage(messageType))
        {
        }

        private static string GetMessage(Type messageType)
        {
            return String.Format("A route has already been registered for message type {0}", messageType.FullName);
        }
    }
}