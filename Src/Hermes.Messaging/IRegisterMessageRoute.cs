using System;

namespace Hermes.Messaging
{
    public interface IRegisterMessageRoute
    {
        IRegisterMessageRoute RegisterRoute(Type messageType, Address endpointAddress);
    }
}