using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Hermes.Messaging.ProcessManagement;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Bus.Transports
{
    public class Dispatcher : IDispatchMessagesToHandlers
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(Dispatcher)); 

        public void DispatchToHandlers(object message, IServiceLocator serviceLocator)
        {
            Type[] contracts = GetMessageContracts(message);
            IEnumerable<HandlerCacheItem> handlerDetails = HandlerCache.GetHandlerDetails(contracts).ToArray();

            if (!handlerDetails.Any())
            {
                throw new InvalidOperationException(String.Format("No handlers could be found for message {0}", message.GetType()));
            }

            var handlers = new List<object>();

            foreach (var messageHandlerDetail in handlerDetails)
            {
                object messageHandler = serviceLocator.GetInstance(messageHandlerDetail.HandlerType);
                messageHandlerDetail.TryHandleMessage(messageHandler, message, contracts);
                handlers.Add(messageHandler);
            }

            if (!handlers.Any())
            {
                throw new InvalidOperationException(String.Format("No handlers could be found for message {0}", message.GetType()));
            }

            SaveProcessManagers(handlers);
        }

        private static Type[] GetMessageContracts(object message)
        {
            var messageType = message.GetType();

            Type[] contracts = Settings.IsCommandType(messageType)
                ? new[] {messageType}
                : message.GetType().GetInterfaces().Union(new[] {messageType}).ToArray();
            return contracts;
        }

        private static void SaveProcessManagers(IEnumerable<object> handlers)
        {
            foreach (var handler in handlers)
            {
                var processManager = handler as ProcessManager;

                if (processManager != null)
                {
                    processManager.Save();
                }
            }
        }
    }
}
