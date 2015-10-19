using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hermes.Logging;
using Hermes.Messaging.MessageHandlerCache;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class DispatchMessagesModule : IModule<MessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(DispatchMessagesModule));

        public bool Process(MessageContext input, Func<bool> next)
        {
            if (input.MessageType == MessageType.Control)
                return next();

            Logger.Debug("Dispatching message {0} to handlers.", input);
            DispatchToHandlers(input.Message);

            return next();
        }

        public virtual void DispatchToHandlers(object message)
        {
            Mandate.ParameterNotNull(message, "message");

            Type[] contracts = MessageContractFactory.GetContracts(message);
            HandlerCacheItem[] handlerDetails = HandlerCache.GetHandlers(contracts);

            DispatchToHandlers(message, handlerDetails, contracts);
        }

        protected virtual void DispatchToHandlers(object message, ICollection<HandlerCacheItem> handlerDetails, Type[] contracts)
        {
            var handlers = new List<object>();
            var stopwatch = new Stopwatch();

            foreach (HandlerCacheItem messageHandlerDetail in handlerDetails)
            {
                stopwatch.Start();
                Logger.Debug("Dispatching {0} to {1}", message.GetType().FullName, messageHandlerDetail.HandlerType.FullName);
                object messageHandler = messageHandlerDetail.TryHandleMessage(message, contracts);
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
