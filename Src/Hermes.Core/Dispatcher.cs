using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Core
{
    public class Dispatcher : IDispatchMessagesToHandlers
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (Dispatcher)); 

        public void DispatchToHandlers(IServiceLocator serviceLocator, object message)
        {
            var handlers = GetHandlers(serviceLocator, message).ToArray();

            if (handlers.Any())
            {
                logger.Verbose("Dispatching message {0} to {1} handlers", message.GetType(), handlers.Length);
                InvokeHandlers(handlers);                
            }
            else
            {
                logger.Warn("No handlers for for message {0}", message.GetType());
            }
        }
      
        private static void InvokeHandlers(IEnumerable<Action> handlers)
        {
            foreach (var handler in handlers)
            {
                handler.Invoke();
            }
        }

        public IEnumerable<Action> GetHandlers(IServiceLocator serviceLocator, object message)
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
