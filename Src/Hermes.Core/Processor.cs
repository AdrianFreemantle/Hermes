using System;
using System.Collections.Generic;
using System.IO;
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

        public Processor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IContainer container, ISendMessages messageSender, IHandleMessageErrors errorProcessor)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.container = container;
            this.messageSender = messageSender;
            this.errorProcessor = errorProcessor;
        }

        public void ProcessEnvelope(MessageEnvelope envelope)
        {
            Logger.Verbose("Processing message {0}", envelope.MessageId);

            try
            {
                Retry.Action(() => TryProcessEnvelope(envelope), OnRetryError, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
            }
            catch (Exception ex)
            {
                errorProcessor.Handle(envelope, ex);
            }
        }

        private void OnRetryError(Exception ex)
        {
            Logger.Warn("Error while processing message, attempting retry : {0}", ex.GetFullExceptionMessage());
        } 

        private void TryProcessEnvelope(MessageEnvelope envelope)
        {
            using (var scope = StartTransactionScope())
            {
                ProcessMessages(ExtractMessages(envelope));
                errorProcessor.RemoveRetryHeaders(envelope);
                messageSender.Send(envelope, Settings.AuditEndpoint);
                scope.Complete();
            }

            Logger.Verbose("Processing completed for message {0}", envelope.MessageId);
        }       

        public void ProcessMessages(IEnumerable<object> messageBodies)
        {
            using (var childContainer = container.BeginLifetimeScope())
            {
                ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                TryProcessMessages(messageBodies, childContainer);
            }
        }

        private void TryProcessMessages(IEnumerable<object> messageBodies, IContainer childContainer)
        {
            try
            {                
                DispatchToHandlers(messageBodies, childContainer);
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
  
        private IEnumerable<object> ExtractMessages(MessageEnvelope envelope)
        {
            if (envelope.Body == null || envelope.Body.Length == 0)
            {
                return new object[0];
            }

            return TryDeserializeMessages(envelope);
        }

        private IEnumerable<object> TryDeserializeMessages(MessageEnvelope envelope)
        {
            try
            {
                using (var stream = new MemoryStream(envelope.Body))
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