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
using Hermes.Transports;
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
        private readonly ISendMessages messageSender;
        private readonly IHandleMessageErrors errorProcessor;

        public event EventHandler<StartedMessageProcessingEventArgs> StartedMessageProcessing;
        public event EventHandler<CompletedMessageProcessingEventArgs> CompletedMessageProcessing;
        public event EventHandler<FailedMessageProcessingEventArgs> FailedMessageProcessing;

        public Processor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IContainer container, ISendMessages messageSender, IHandleMessageErrors errorProcessor)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.container = container;
            this.messageSender = messageSender;
            this.errorProcessor = errorProcessor;
        }

        public void ProcessEnvelope(TransportMessage transportMessage)
        {
            Logger.Verbose("Processing message {0}", transportMessage.MessageId);

            object[] messages = ExtractMessages(transportMessage).ToArray();
            RaiseStartedProcessingMessageEvent(transportMessage, messages);

            try
            {
                Retry.Action(() => TryProcessEnvelope(transportMessage, messages), OnRetryError, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
                RaiseMessageProcessingCompletedEvent(transportMessage, messages);
            }
            catch (Exception ex)
            {
                RaiseMessageProcessingFailedEvent(ex);
                errorProcessor.Handle(transportMessage, ex);
            }
        }

        private void RaiseMessageProcessingFailedEvent(Exception ex)
        {
            if (FailedMessageProcessing != null)
            {
                FailedMessageProcessing(this, new FailedMessageProcessingEventArgs(ex));
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

        private void OnRetryError(Exception ex)
        {
            Logger.Warn("Error while processing message, attempting retry : {0}", ex.GetFullExceptionMessage());
        }

        private void TryProcessEnvelope(TransportMessage transportMessage, IEnumerable<object> messages)
        {
            using (var scope = StartTransactionScope())
            {
                ProcessMessages(messages);
                errorProcessor.RemoveRetryHeaders(transportMessage);
                messageSender.Send(transportMessage, Settings.AuditEndpoint);
                scope.Complete();
            }

            Logger.Verbose("Processing completed for transportMessage {0}", transportMessage.MessageId);
        }       

        public void ProcessMessages(IEnumerable<object> messages)
        {
            using (var childContainer = container.BeginLifetimeScope())
            {
                ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                TryProcessMessages(messages, childContainer);
            }
        }

        private void TryProcessMessages(IEnumerable<object> messages, IContainer childContainer)
        {
            try
            {                
                DispatchToHandlers(messages, childContainer);
                CommitUnitsOfWork(childContainer.GetAllInstances<IManageUnitOfWork>());
            }
            catch
            {
                RollBackUnitsOfWork(childContainer.GetAllInstances<IManageUnitOfWork>());
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
                ? TransactionScopeUtils.Begin(TransactionScopeOption.RequiresNew) 
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
            foreach (var unitOfWork  in unitsOfWork)
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
    }
}