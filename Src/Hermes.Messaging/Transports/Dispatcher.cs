using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Hermes.Messaging.ProcessManagement;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class Dispatcher : IDispatchMessagesToHandlers
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(Dispatcher));

        public virtual void DispatchToHandlers(object message, IServiceLocator serviceLocator)
        {
            Type[] contracts = GetMessageContracts(message);
            HandlerCacheItem[] handlerDetails = HandlerCache.GetHandlerDetails(contracts);
            DispatchToHandlers(message, serviceLocator, handlerDetails, contracts);
        }

        protected virtual Type[] GetMessageContracts(object message)
        {
            var messageType = message.GetType();

            Type[] contracts = Settings.IsCommandType(messageType)
                ? new[] { messageType }
                : message.GetType().GetInterfaces().Union(new[] { messageType }).ToArray();
            return contracts;
        }

        protected virtual void DispatchToHandlers(object message, IServiceLocator serviceLocator, IEnumerable<HandlerCacheItem> handlerDetails, Type[] contracts)
        {
            var handlers = new List<object>();

            foreach (var messageHandlerDetail in handlerDetails)
            {
                TryHandleMessage(message, serviceLocator, messageHandlerDetail, contracts, handlers);
            }

            SaveProcessManagers(handlers);
        }

        protected virtual void TryHandleMessage(object message, IServiceLocator serviceLocator, HandlerCacheItem messageHandlerDetail, IEnumerable<Type> contracts, List<object> handlers)
        {
            try
            {
                logger.Verbose("Dispatching message {0}", message.ToString());
                object messageHandler = messageHandlerDetail.TryHandleMessage(serviceLocator, message, contracts);
                handlers.Add(messageHandler);
            }
            catch (ProcessManagerDataNotFoundException ex)
            {
                logger.Warn(ex.Message);
            }
        }

        protected virtual void SaveProcessManagers(IEnumerable<object> handlers)
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
