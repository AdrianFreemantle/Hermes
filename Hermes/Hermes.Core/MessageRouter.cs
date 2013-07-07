using System;
using System.Collections.Generic;

namespace Hermes.Core
{
    public class MessageRouter : IRouteMessageToEndpoint, IRegisterMessageRoute
    {
        readonly IDictionary<Type, Address> routes;

        public MessageRouter()
        {
            routes = new Dictionary<Type, Address>();
        }

        public Address GetDestinationFor(Type messageType)
        {
            if (routes.ContainsKey(messageType))
            {
                return routes[messageType];
            }

            throw new RouteNotDefinedException(messageType);
        }

        public IRegisterMessageRoute RegisterRoute(Type messageType, Address endpointAddress)
        {
            if (routes.ContainsKey(messageType))
            {
                routes[messageType] = endpointAddress;
            }
            else
            {
                routes.Add(messageType, endpointAddress);   
            }

            return this;
        }
    }
}