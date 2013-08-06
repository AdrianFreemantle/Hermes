using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Serialization;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Core
{
    public class MessageProcessor : IProcessMessages
    {
        private readonly ISerializeMessages messageSerializer;
        private readonly IDispatchMessagesToHandlers messageDispatcher;
        private readonly IObjectBuilder objectBuilder;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageDispatcher));

        readonly ThreadLocal<IServiceLocator> localServiceLocator = new ThreadLocal<IServiceLocator>();

        public MessageProcessor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IObjectBuilder objectBuilder)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.objectBuilder = objectBuilder;
        }

        public void Process(MessageEnvelope envelope)
        {
            Logger.Debug("Processing messsage {0}", envelope.MessageId);
            IEnumerable<object> messageBodies = ExtractMessages(envelope);

            try
            {
                TryProcess(messageBodies);
            }
            catch (Exception ex)
            {
                Logger.Error("Processing failed for message {0}: {1}", envelope.MessageId, ex.Message);
                throw new MessageProcessingFailedException(envelope, ex);
            }
            finally
            {
                localServiceLocator.Value = null;
            }
        }

        private void TryProcess(IEnumerable<object> messageBodies)
        {
            using (var childBuilder = objectBuilder.BeginLifetimeScope())
            {
                using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Suppress))
                {
                    localServiceLocator.Value = childBuilder;
                    DispatchToHandlers(messageBodies);
                    CommitUnitsOfWork(childBuilder.GetAllInstances<IManageUnitOfWork>());
                    scope.Complete();
                }
            }
        }

        public void DispatchToHandlers(IEnumerable<object> messageBodies)
        {
            foreach (var body in messageBodies)
            {
                messageDispatcher.DispatchToHandlers(localServiceLocator.Value, body);
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