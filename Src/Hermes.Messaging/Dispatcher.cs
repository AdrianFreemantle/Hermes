using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.ProcessManagement;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging
{
    public class Dispatcher : IDispatchMessagesToHandlers
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(Dispatcher)); 
        private readonly IServiceLocator serviceLocator;

        public Dispatcher(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void DispatchToHandlers(object message)
        {
            object[] handlers = GetHandlers(message).ToArray();

            ValidateMessageHandlers(message, handlers);

            if (handlers.Any())
            {
                logger.Verbose("Dispatching message {0} to {1} handlers", message.GetType(), handlers.Length);
                InvokeHandlers(handlers, message);
                TrySaveSaga(handlers);
            }
            else
            {
                if(Settings.IsCommandType(message.GetType()))
                {
                    throw new InvalidOperationException(String.Format("No handlers could be found for command {0}", message.GetType()));
                }

                logger.Warn("No handlers for for message {0}", message.GetType());
            }
        }

        private static void ValidateMessageHandlers(object message, IEnumerable<object> handlers)
        {
            if (Settings.IsCommandType != null && Settings.IsCommandType(message.GetType()) && handlers.Count() != 1)
            {
                throw new InvalidOperationException("A command must have one and only one handler allocated to it.");
            }
        }

        private static void TrySaveSaga(IEnumerable<object> handlers)
        {
            foreach (var handler in handlers)
            {
                var saga = handler as ProcessManager;

                if (saga != null)
                {
                    saga.Save();
                }
            }
        }

        private static void InvokeHandlers(IEnumerable<object> handlers, object message)
        {
            foreach (var action in handlers.Select(h => CreateHandlerAction(message, h)))
            {
                action.Invoke();
            }
        }

        private IEnumerable<object> GetHandlers(object message)
        {
            Type handlerGenericType = typeof(IHandleMessage<>);
            Type handlerType = handlerGenericType.MakeGenericType(new[] { message.GetType() });
            return serviceLocator.GetAllInstances(handlerType).ToArray();
        }

        private static Action CreateHandlerAction(object message, object handler)
        {
            return () => ((dynamic)handler).Handle((dynamic)message);
        }
    }
}
