using System;
using System.Collections.Generic;
using Hermes.Messages;
using Hermes.Transports;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Core
{
    public class MessageDispatcher : IDispatchMessagesToHandlers
    {
        private readonly IServiceLocator serviceLocator;

        public MessageDispatcher(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void DispatchToHandlers(object message)
        {
            Type handlerGenericType = typeof(IHandleMessage<>);
            Type handlerType = handlerGenericType.MakeGenericType(new[] { message.GetType() });
            IEnumerable<object> handlers = serviceLocator.GetAllInstances(handlerType);

            foreach (var handler in handlers)
            {
                ((dynamic)handler).Handle((dynamic)message);
            }
        }

        public void DispatchToHandlers(IEnumerable<object> messages)
        {
            foreach (var message in messages)
            {
                DispatchToHandlers(message);
            }
        }
    }
}
