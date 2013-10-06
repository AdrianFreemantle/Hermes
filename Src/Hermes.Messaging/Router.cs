using System;
using System.Collections.Generic;

using Hermes.Messaging.Routing;

namespace Hermes.Messaging
{
    public class Router : IRouteMessageToEndpoint, IRegisterMessageRoute
    {
        readonly IDictionary<Type, Address> routes;

        public Router()
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
                throw new RouteAlreadyDefinedException(messageType);
            }
            
            routes.Add(messageType, endpointAddress);

            return this;
        }
    }


}