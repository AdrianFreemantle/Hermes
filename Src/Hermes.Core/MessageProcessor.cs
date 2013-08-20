using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using Hermes.Configuration;
using Hermes.Core.Deferment;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Serialization;
using Hermes.Transports;
using Microsoft.Practices.ServiceLocation;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Core
{

    public class RetryHeaders
    {
        public const string Count = "Hermes.Retry.Count";
    }

    public class MessageProcessor : IProcessMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageProcessor));

        private readonly ISerializeMessages messageSerializer;
        private readonly IDispatchMessagesToHandlers messageDispatcher;
        private readonly IContainer container;
        private readonly ISendMessages messageSender;

        public MessageProcessor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IContainer container, ISendMessages messageSender)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.container = container;
            this.messageSender = messageSender;
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

                int retryCount = 0;

                if (envelope.Headers.ContainsKey(RetryHeaders.Count))
                {
                    retryCount = Int32.Parse(envelope.Headers[RetryHeaders.Count]);
                }

                if (retryCount >= 3)
                {
                    messageSender.Send(envelope, Settings.ErrorEndpoint);
                }
                else
                {
                    envelope.Headers[RetryHeaders.Count] = (++retryCount).ToString(CultureInfo.InvariantCulture);
                    envelope.Headers[TimeoutHeaders.Expire] = DateTime.UtcNow.Add(TimeSpan.FromSeconds(15)).ToWireFormattedString();
                    envelope.Headers[TimeoutHeaders.RouteExpiredTimeoutTo] = Settings.ThisEndpoint.ToString();
                    messageSender.Send(envelope, Settings.DefermentEndpoint);
                }
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