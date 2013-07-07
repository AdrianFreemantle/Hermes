using System;

namespace Hermes
{
    public interface IRegisterMessageRoute
    {
        IRegisterMessageRoute RegisterRoute(Type messageType, Address endpointAddress);
    }
}