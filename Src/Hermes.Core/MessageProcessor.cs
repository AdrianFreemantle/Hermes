using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Serialization;
using Microsoft.Practices.ServiceLocation;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Core
{
    public class MessageProcessor : IProcessMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageProcessor));

        private readonly ISerializeMessages messageSerializer;
        private readonly IDispatchMessagesToHandlers messageDispatcher;
        private readonly IContainer container;

        public MessageProcessor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IContainer container)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.container = container;
        }

        public void Process(MessageEnvelope envelope)
        {
            Logger.Debug("Processing message {0}", envelope.MessageId);
            IEnumerable<object> messageBodies = ExtractMessages(envelope);

            try
            {
                ProcessMessages(messageBodies);
            }
            catch (Exception ex)
            {
                Logger.Error("Processing failed for message {0}: {1}", envelope.MessageId, ex.Message);
                throw new MessageProcessingFailedException(envelope, ex);
            }
            finally
            {
                ServiceLocator.Current.SetServiceProvider(null);
            }
        }

        private void ProcessMessages(IEnumerable<object> messageBodies)
        {
            using (var childContainer = container.BeginLifetimeScope())
            using (var scope = StartTransactionScope())
            {
                ServiceLocator.Current.SetServiceProvider(childContainer);
                DispatchToHandlers(messageBodies, childContainer);
                CommitUnitsOfWork(childContainer.GetAllInstances<IManageUnitOfWork>());
                scope.Complete();
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
            catch (Exception e)
            {
                throw new SerializationException("Could not deserialize message.", e);
            }
        }
    }
}