using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Messages.Attributes;

namespace Hermes.Core
{
    public class MessageDispatcher : IDispatchMessagesToHandlers
    {
        private readonly IBuildMessageHandlers handlerFactory;

        public MessageDispatcher(IBuildMessageHandlers handlerFactory)
        {
            this.handlerFactory = handlerFactory;
        }

        public void DispatchToHandlers(object message)
        {
            Console.WriteLine("{1}: Dispatching message {0}", message.GetType(), System.Threading.Thread.CurrentThread.ManagedThreadId);

            var handlers = handlerFactory.GetMessageHandlers(message.GetType());
            var attribute = GetRetryAttribute(message);

            if (attribute == null)
            {
                DispatchMessage(message, handlers);
            }
            else
            {
                DispatchMessageWithRetry(message, handlers, attribute);
            }
        }

        private static RetryAttribute GetRetryAttribute(object message)
        {
            return Attribute.GetCustomAttributes(message.GetType())
                                     .FirstOrDefault(a => a is RetryAttribute) as RetryAttribute;
        }

        private static void DispatchMessage(object message, IEnumerable<object> handlers)
        {
            foreach (var handler in handlers)
            {
                RouteMessageToHandleMethod(message, handler);
            }
        }

        private static void DispatchMessageWithRetry(object message, IEnumerable<object> handlers, RetryAttribute attribute)
        {
            foreach (var handler in handlers)
            {
                Action<Exception> onError = ex => Console.WriteLine("Retrying message after error: {0}", ex.Message);
                var dispatch = new Action(() => RouteMessageToHandleMethod(message, handler));
                Retry.Action(dispatch, onError, attribute.RetryCount, attribute.RetryMilliseconds);
            }
        }

        private static void RouteMessageToHandleMethod(object message, object handler)
        {
            ((dynamic)handler).Handle((dynamic)message);
        }
    }
}
