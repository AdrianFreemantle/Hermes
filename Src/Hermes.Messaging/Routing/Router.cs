using System;
using System.Collections.Generic;

using Hermes.Logging;
using Hermes.Messaging.Pipeline.Modules;

namespace Hermes.Messaging.Routing
{
    public class Router : IRouteMessageToEndpoint, IRegisterMessageRoute
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Router));
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
            
            Logger.Debug("Registering route {0} for message type {1}", endpointAddress, messageType.FullName);
            routes.Add(messageType, endpointAddress);

            return this;
        }
    }


}