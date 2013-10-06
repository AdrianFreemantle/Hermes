using System;

namespace Hermes.Messaging.Routing
{
    public interface IRegisterMessageRoute
    {
        IRegisterMessageRoute RegisterRoute(Type messageType, Address endpointAddress);
    }
}