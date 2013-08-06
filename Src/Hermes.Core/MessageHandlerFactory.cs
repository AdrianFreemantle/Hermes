using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Messages;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Core
{
    public class MessageHandlerFactory : IBuildMessageHandlers
    {
        public IEnumerable<Action> GetHandler(IServiceLocator serviceLocator, object message)
        {
            Type handlerGenericType = typeof(IHandleMessage<>);
            Type handlerType = handlerGenericType.MakeGenericType(new[] { message.GetType() });
            var handlers = serviceLocator.GetAllInstances(handlerType);
      
            return handlers.Select(h => CreateHandlerAction(message, h));
        }

        private static Action CreateHandlerAction(object message, object handler)
        {
            return () => ((dynamic)handler).Handle((dynamic)message);
        }
    }
}