using System;
using System.Collections.Generic;

using Hermes.Logging;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Hermes.Messaging.ProcessManagement;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class Dispatcher : IDispatchMessagesToHandlers
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Dispatcher));

        public virtual void DispatchToHandlers(object message, IServiceLocator serviceLocator)
        {
            Type[] contracts = message.GetContracts();
            HandlerCacheItem[] handlerDetails = HandlerCache.GetHandlerDetails(contracts);
            DispatchToHandlers(message, serviceLocator, handlerDetails, contracts);
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
                Logger.Debug("Dispatching {0} to {1}", message.GetType().FullName, messageHandlerDetail.HandlerType.FullName);
                object messageHandler = messageHandlerDetail.TryHandleMessage(serviceLocator, message, contracts);
                handlers.Add(messageHandler);
            }
            catch (ProcessManagerDataNotFoundException ex)
            {
                Logger.Warn(ex.Message);
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
