﻿using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messages.Attributes;

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
            logger.Debug("Dispatching message {0}", message.GetType());
            IEnumerable<Action> handlers = handlerFactory.GetHandler(serviceLocator, message);
            InvokeHandlers(message, handlers);
        }

        private void InvokeHandlers(object message, IEnumerable<Action> handlers)
        {
            var attribute = GetRetryAttribute(message);

            if (attribute == null)
            {
                InvokeHandlers(handlers);
            }
            else
            {
                InvokeHandlersWithRetry(handlers, attribute);
            }
        }

        private static void InvokeHandlers(IEnumerable<Action> handlers)
        {
            foreach (var handler in handlers)
            {
                handler.Invoke();
            }
        }

        private void InvokeHandlersWithRetry(IEnumerable<Action> handlers, RetryAttribute attribute)
        {
            foreach (var handler in handlers)
            {
                Retry.Action(handler, OnRetryError, attribute.RetryCount, attribute.RetryMilliseconds);
            }
        }
      
        private void OnRetryError(Exception ex)
        {
            logger.Warn("Error while dispatching message, attempting retry: {0}", ex.Message);
        }

        private static RetryAttribute GetRetryAttribute(object message)
        {
            return Attribute.GetCustomAttributes(message.GetType())
                            .FirstOrDefault(a => a is RetryAttribute) as RetryAttribute;
        }
    }
}
