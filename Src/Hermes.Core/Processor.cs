using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Transactions;
using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Serialization;
using Microsoft.Practices.ServiceLocation;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Core
{
    public class Processor : IProcessMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Processor));

        private readonly ISerializeMessages messageSerializer;
        private readonly IDispatchMessagesToHandlers messageDispatcher;
        private readonly IContainer container;

        public event EventHandler<StartedMessageProcessingEventArgs> StartedMessageProcessing;
        public event EventHandler<CompletedMessageProcessingEventArgs> CompletedMessageProcessing;
        public event EventHandler<FailedMessageProcessingEventArgs> FailedMessageProcessing;

        public Processor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IContainer container)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.container = container;
        }

        public void ProcessTransportMessage(TransportMessage transportMessage)
        {
            Logger.Verbose("Processing transport message {0}", transportMessage.MessageId);
           
            try
            {
                object[] messages = ExtractMessages(transportMessage).ToArray();
                RaiseStartedProcessingMessageEvent(transportMessage, messages);
                TryProcessEnvelope(transportMessage, messages);
                RaiseMessageProcessingCompletedEvent(transportMessage, messages);
            }
            catch (Exception ex)
            {
                Logger.Error("Error while processing transport message {0} {1}", transportMessage.MessageId, ex.GetFullExceptionMessage());
                RaiseMessageProcessingFailedEvent(ex, transportMessage);
            }
        }

        private void TryProcessEnvelope(TransportMessage transportMessage, IEnumerable<object> messages)
        {
            TestError.Throw();

            using (var scope = StartTransactionScope())
            {
                try
                {
                    Retry.Action(() => ProcessMessages(messages), OnRetryError, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
                    Logger.Verbose("Processing completed for transportMessage {0}", transportMessage.MessageId);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while processing transport message {0} {1}", transportMessage.MessageId, ex.GetFullExceptionMessage());
                    RaiseMessageProcessingFailedEvent(ex, transportMessage);
                }
                
                TestError.Throw();
                scope.Complete();
            }
        }

        private void ProcessMessages(IEnumerable<object> messages)
        {
            using (var childContainer = container.BeginLifetimeScope())
            {
                ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                TryProcessMessages(messages, childContainer);
            }
        }

        private void TryProcessMessages(IEnumerable<object> messages, IContainer childContainer)
        {
            var unitsOfWork = childContainer.GetAllInstances<IManageUnitOfWork>().ToArray();
            var outgoingMessages = childContainer.GetInstance<IManageOutgoingMessages>();

            try
            {                
                DispatchToHandlers(messages, childContainer);
                CommitUnitsOfWork(unitsOfWork);
                outgoingMessages.Send();
            }
            catch 
            {
                Logger.Verbose("Rolling back units of work");
                RollBackUnitsOfWork(unitsOfWork);
                throw;
            }
            finally
            {
                ServiceLocator.Current.SetCurrentLifetimeScope(null);
            }
        }

        private static TransactionScope StartTransactionScope()
        {
            return Settings.UseDistributedTransaction 
                ? TransactionScopeUtils.Begin(TransactionScopeOption.Required) 
                : TransactionScopeUtils.Begin(TransactionScopeOption.Suppress);
        }

        private void DispatchToHandlers(IEnumerable<object> messageBodies, IServiceLocator serviceLocator)
        {
            foreach (var body in messageBodies)
            {
                messageDispatcher.DispatchToHandlers(serviceLocator, body);  
            }
        }

        private static void CommitUnitsOfWork(IEnumerable<IManageUnitOfWork> unitsOfWork)
        {
            Logger.Verbose("Committing units of work");

            foreach (var unitOfWork in unitsOfWork.Reverse())
            {
                unitOfWork.Commit();
            }
        }

        private static void RollBackUnitsOfWork(IEnumerable<IManageUnitOfWork> unitsOfWork)
        {
            foreach (var unitOfWork in unitsOfWork)
            {
                unitOfWork.Rollback();
            }
        }

        private void OnRetryError(Exception ex)
        {
            if (ex is HermesTestingException)
            {
                Logger.Verbose("Attempting retry due to testing exception");
                return;
            }

            Logger.Warn("Error while processing message, attempting retry : {0}", ex.GetFullExceptionMessage());
        }
  
        private IEnumerable<object> ExtractMessages(TransportMessage transportMessage)
        {
            if (transportMessage.Body == null || transportMessage.Body.Length == 0)
            {
                return new object[0];
            }

            return TryDeserializeMessages(transportMessage.Body);
        }

        private IEnumerable<object> TryDeserializeMessages(byte[] messages)
        {
            try
            {
                using (var stream = new MemoryStream(messages))
                {
                    return messageSerializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException("Could not deserialize message.", ex);
            }
        }


        private void RaiseMessageProcessingFailedEvent(Exception ex, TransportMessage transportMessage)
        {
            if (FailedMessageProcessing != null)
            {
                FailedMessageProcessing(this, new FailedMessageProcessingEventArgs(ex, transportMessage));
            }
        }

        private void RaiseMessageProcessingCompletedEvent(TransportMessage transportMessage, object[] messages)
        {
            if (CompletedMessageProcessing != null)
            {
                CompletedMessageProcessing(this, new CompletedMessageProcessingEventArgs(transportMessage, messages));
            }
        }

        private void RaiseStartedProcessingMessageEvent(TransportMessage transportMessage, object[] messages)
        {
            if (StartedMessageProcessing != null)
            {
                StartedMessageProcessing(this, new StartedMessageProcessingEventArgs(transportMessage, messages));
            }
        }

    }
}