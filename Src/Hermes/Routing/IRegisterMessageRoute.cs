using System;

using Hermes.Messaging;

namespace Hermes.Routing
{
    public interface IRegisterMessageRoute
    {
        IRegisterMessageRoute RegisterRoute(Type messageType, Address endpointAddress);
    }
}