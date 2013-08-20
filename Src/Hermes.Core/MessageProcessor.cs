using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Transactions;
using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Serialization;
using Hermes.Transports;
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
        private readonly ISendMessages messageSender;

        public MessageProcessor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IContainer container, ISendMessages messageSender)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.container = container;
            this.messageSender = messageSender;
        }

        public void ProcessEnvelope(MessageEnvelope envelope)
        {
            Logger.Verbose("Processing message {0}", envelope.MessageId);

            try
            {
                TryProcessEnvelope(envelope);
                Logger.Verbose("Processing completed for message {0}", envelope.MessageId);
            }
            catch (Exception ex)
            {
                HandleProcessingError(envelope, ex);
            }
        }

        private void HandleProcessingError(MessageEnvelope envelope, Exception ex)
        {
            Logger.Warn("Error while processing message {0} : {1}", envelope.MessageId, ex.Message);
            int retryCount = 0;

            if (envelope.Headers.ContainsKey(MessageHeaders.Count))
            {
                retryCount = Int32.Parse(envelope.Headers[MessageHeaders.Count]);
            }

            if (++retryCount > 3)
            {
                SendToErrorQueue(envelope, ex);
            }
            else
            {
                SendToRetryQueue(envelope, retryCount);
            }
        }

        private void SendToRetryQueue(MessageEnvelope envelope, int retryCount)
        {
            Logger.Warn("Sending message {0} to retry queue: attempt {1}", envelope.MessageId, retryCount);
            envelope.Headers[MessageHeaders.Count] = (retryCount).ToString(CultureInfo.InvariantCulture);
            envelope.Headers[MessageHeaders.Expire] = DateTime.UtcNow.Add(TimeSpan.FromSeconds(2)).ToWireFormattedString();
            envelope.Headers[MessageHeaders.RouteExpiredTimeoutTo] = Settings.ThisEndpoint.ToString();
            messageSender.Send(envelope, Settings.DefermentEndpoint);
        }

        private void SendToErrorQueue(MessageEnvelope envelope, Exception ex)
        {
            Logger.Error("Final retry failed for message {0}.", envelope.MessageId);
            envelope.Headers[MessageHeaders.Failed] = GetExceptionMessage(ex);
            messageSender.Send(envelope, Settings.ErrorEndpoint);
        }

        private static string GetExceptionMessage(Exception ex)
        {
            var exceptionMessage = new StringBuilder();
            var currentException = ex;

            while (currentException != null)
            {
                exceptionMessage.AppendLine(String.Format("{0}\n", ex));
                currentException = currentException.InnerException;
            }

            return exceptionMessage.ToString();
        }

        private void TryProcessEnvelope(MessageEnvelope envelope)
        {
            using (var scope = StartTransactionScope())
            {
                ProcessMessages(ExtractMessages(envelope));
                RemoveRetryHeaders(envelope);
                messageSender.Send(envelope, Settings.AuditEndpoint);
                scope.Complete();
            }
        }

        private void RemoveRetryHeaders(MessageEnvelope envelope)
        {
            envelope.Headers.Remove(MessageHeaders.Count);
            envelope.Headers.Remove(MessageHeaders.Expire);
            envelope.Headers.Remove(MessageHeaders.RouteExpiredTimeoutTo);
            envelope.Headers.Remove(MessageHeaders.Failed);
        }

        public void ProcessMessages(IEnumerable<object> messageBodies)
        {
            using (var childContainer = container.BeginLifetimeScope())
            {
                TryProcessMessages(messageBodies, childContainer);
            }
        }

        private void TryProcessMessages(IEnumerable<object> messageBodies, IContainer childContainer)
        {
            try
            {
                ServiceLocator.Current.SetServiceProvider(childContainer);
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
                ServiceLocator.Current.SetServiceProvider(null);
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
            catch (Exception e)
            {
                throw new SerializationException("Could not deserialize message.", e);
            }
        }
    }
}