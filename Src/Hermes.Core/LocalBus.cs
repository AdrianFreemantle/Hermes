using System;
using System.Collections.Generic;
using System.Transactions;

using Hermes.Configuration;
using Hermes.Logging;
using Hermes.Messaging;

using Microsoft.Practices.ServiceLocation;

using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Core
{
    public class LocalBus : IInMemoryBus
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(MessageBus));
        private readonly IProcessMessages messageProcessor;
        readonly IDispatchMessagesToHandlers messageDispatcher;

        public LocalBus(IProcessMessages messageProcessor, IDispatchMessagesToHandlers messageDispatcher)
        {
            this.messageProcessor = messageProcessor;
            this.messageDispatcher = messageDispatcher;
        }

        void IInMemoryBus.Raise(params object[] events)
        {
            var serviceLocator = ServiceLocator.Current.GetService<IServiceLocator>();
            Raise(events, serviceLocator);
        }

        private void Raise(IEnumerable<object> events, IServiceLocator serviceLocator)
        {
            foreach (var @event in events)
            {
                messageDispatcher.DispatchToHandlers(serviceLocator, @event);
            }
        }

        void IInMemoryBus.Execute(params object[] commands)
        {
            Retry.Action(() => Execute(commands), OnRetryError, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
        }

        private void Execute(IEnumerable<object> commands)
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.RequiresNew))
            {
                messageProcessor.ProcessMessages(commands);
                scope.Complete();
            }
        }

        private void OnRetryError(Exception ex)
        {
            logger.Warn("Error while processing in memory message, attempting retry : {0}", ex.GetFullExceptionMessage());
        } 
    }
}