using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hermes.Logging;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class Dispatcher : IDispatchMessagesToHandlers
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Dispatcher));

        public virtual void DispatchToHandlers(object message, IServiceLocator serviceLocator)
        {
            Mandate.ParameterNotNull(message, "message");
            Mandate.ParameterNotNull(serviceLocator, "serviceLocator");

            Type[] contracts = message.GetContracts();
            HandlerCacheItem[] handlerDetails = HandlerCache.GetHandlers(contracts);

            
            DispatchToHandlers(message, serviceLocator, handlerDetails, contracts);
        }

        protected virtual void DispatchToHandlers(object message, IServiceLocator serviceLocator, ICollection<HandlerCacheItem> handlerDetails, Type[] contracts)
        {
            var handlers = new List<object>();
            var stopwatch = new Stopwatch();

            foreach (HandlerCacheItem messageHandlerDetail in handlerDetails)
            {
                stopwatch.Start();
                Logger.Debug("Dispatching {0} to {1}", message.GetType().FullName, messageHandlerDetail.HandlerType.FullName);
                object messageHandler = messageHandlerDetail.TryHandleMessage(serviceLocator, message, contracts);
                stopwatch.Stop();
                handlers.Add(messageHandler);
                Logger.Debug("Message {0} was handled by {1} in {2}", message.GetType().FullName, messageHandlerDetail.HandlerType.FullName, stopwatch.Elapsed);
            }

            SaveProcessManagers(handlers);
        }

        protected virtual void SaveProcessManagers(IEnumerable<object> handlers)
        {
            foreach (var handler in handlers)
            {
                var processManager = handler as IProcessManager;

                if (processManager != null)
                {
                    processManager.Save();
                }
            }
        }      
    }
}
