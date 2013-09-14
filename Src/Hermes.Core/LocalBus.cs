using System;
using System.Collections.Generic;
using System.Transactions;

using Hermes.Configuration;
using Hermes.Logging;
using Hermes.Messaging;

namespace Hermes.Core
{
    public class LocalBus : IInMemoryBus
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(MessageBus));
        private readonly IProcessMessages messageProcessor;

        public LocalBus(IProcessMessages messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }

        void IInMemoryBus.Raise(params object[] events)
        {
            Retry.Action(() => Raise(events), OnRetryError, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
        }

        private void Raise(IEnumerable<object> events)
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.RequiresNew))
            {
                messageProcessor.ProcessMessages(events);
                scope.Complete();
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