using System;
using System.Collections.Generic;

using Hermes.Messages;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Core
{
    public class MessageHandlerFactory : IBuildMessageHandlers
    {
        private readonly IServiceLocator serviceLocator;

        public MessageHandlerFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public IEnumerable<object> GetMessageHandlers(Type messageType)
        {
            Type handlerGenericType = typeof(IHandleMessage<>);
            Type handlerType = handlerGenericType.MakeGenericType(new[] { messageType });
            return serviceLocator.GetAllInstances(handlerType);
        }
    }
}