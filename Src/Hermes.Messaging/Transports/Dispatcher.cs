using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Ioc;
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
            Mandate.ParameterNotNull(message, "message");
            Mandate.ParameterNotNull(serviceLocator, "serviceLocator");

            Type[] contracts = message.GetContracts();
            HandlerCacheItem[] handlerDetails = HandlerCache.GetHandlerDetails(contracts);
            DispatchToHandlers(message, serviceLocator, handlerDetails, contracts);
        }

        protected virtual void DispatchToHandlers(object message, IServiceLocator serviceLocator, ICollection<HandlerCacheItem> handlerDetails, Type[] contracts)
        {
            var handlers = new List<object>();

            foreach (HandlerCacheItem messageHandlerDetail in handlerDetails)
            {
                Logger.Debug("Dispatching {0} to {1}", message.GetType().FullName, messageHandlerDetail.HandlerType.FullName);
                object messageHandler = messageHandlerDetail.TryHandleMessage(serviceLocator, message, contracts);
                handlers.Add(messageHandler);
            }

            SaveProcessManagers(handlers);
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

    public class CommandValidator
    {
        public void ValidateCommand(object command, IServiceLocator serviceLocator)
        {
            Mandate.ParameterNotNull(command, "command");
            Mandate.ParameterNotNull(serviceLocator, "serviceLocator");

            var results = DataAnnotationValidator.Validate(command);

            if (results.Any())
                throw new CommandValidationException(results);


            object validator;
            var validatorType = typeof(IValidateCommand<>).MakeGenericType(command.GetType());

            serviceLocator.TryGetInstance(validatorType, out validator);
            ((dynamic)validator).Validate(command);
        }
    }
}
