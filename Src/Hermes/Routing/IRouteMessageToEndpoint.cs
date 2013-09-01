using System;

using Hermes.Messaging;

namespace Hermes.Routing
{
    public interface IRouteMessageToEndpoint
    {
        /// <summary>
        /// Gets the owner/destination for the given message
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        Address GetDestinationFor(Type messageType);
    }
}
