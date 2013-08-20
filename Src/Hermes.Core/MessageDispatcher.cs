using System;
using System.Collections.Generic;

using Hermes.Logging;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Core
{
    public class MessageDispatcher : IDispatchMessagesToHandlers
    {
        private readonly IBuildMessageHandlers handlerFactory;
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (MessageDispatcher)); 

        public MessageDispatcher(IBuildMessageHandlers handlerFactory)
        {
            this.handlerFactory = handlerFactory;
        }

        public void DispatchToHandlers(IServiceLocator serviceLocator, object message)
        {
            logger.Verbose("Dispatching message {0}", message.GetType());
            IEnumerable<Action> handlers = handlerFactory.GetHandlers(serviceLocator, message);
            InvokeHandlers(handlers);
        }
      
        private static void InvokeHandlers(IEnumerable<Action> handlers)
        {
            foreach (var handler in handlers)
            {
                handler.Invoke();
            }
        }
    }
}
