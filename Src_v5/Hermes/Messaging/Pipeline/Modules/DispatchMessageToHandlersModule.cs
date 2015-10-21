using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using Hermes.Equality;
using Hermes.Logging;
using Hermes.Messaging.MessageHandlerCache;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class DispatchMessageToHandlersModule : IModule<MessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(DispatchMessageToHandlersModule));
        private static readonly TypeEqualityComparer EqualityComparer = new TypeEqualityComparer();


        public bool Process(MessageContext input, Func<bool> next)
        {
            if (!input.IsControlMessage())
            {
                Logger.Debug("Dispatching message {0} to handlers.", input);
                DispatchToHandlers(input);
            }
            
            return next();
        }

        protected virtual void DispatchToHandlers(MessageContext input)
        {
            Mandate.ParameterNotNull(input, "input");

            Type[] contracts = GetContracts(input);
            HandlerCacheItem[] handlerDetails = HandlerCache.GetHandlers(contracts);

            DispatchToHandlers(input.Message, handlerDetails, contracts);
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

        protected virtual Type[] GetContracts(MessageContext context)
        {
            Type messageType = context.Message.GetType();

            if (MessageTypeDefinition.IsCommand(messageType))
                return GetSingleContract(messageType);

            if (MessageTypeDefinition.IsEvent(messageType))
                return GetEventContracts(messageType);

            throw new InvalidMessageContractException(String.Format("The type {0} contains no known message contract types.", messageType.FullName));
        }

        private static Type[] GetSingleContract(Type messageType)
        {
            return new[] { messageType };
        }

        private static Type[] GetEventContracts(Type messageType)
        {
            return messageType
                .GetInterfaces()
                .Union(new[] { messageType })
                .Distinct(EqualityComparer)
                .ToArray();
        }
    }
}
