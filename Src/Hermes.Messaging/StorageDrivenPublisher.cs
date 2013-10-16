using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Storage;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public class StorageDrivenPublisher : IPublishMessages
    {
        private readonly IStoreSubscriptions subscriptionStorage;
        private readonly ITransportMessageFactory messageFactory;
        private readonly ISendMessages messageSender;

        public StorageDrivenPublisher(ISendMessages messageSender, IStoreSubscriptions subscriptionStorage, ITransportMessageFactory messageFactory)
        {
            this.messageSender = messageSender;
            this.subscriptionStorage = subscriptionStorage;
            this.messageFactory = messageFactory;
        }

        public bool Publish(params object[] messages)
        {
            return Publish(Guid.Empty, messages);
        }

        public bool Publish(Guid correlationId, params object[] messages)
        {
            GuardPublisher(messages);

            Type[] messageTypes = messages.Select(o => o.GetType()).ToArray();
            Address[] subscribers = subscriptionStorage.GetSubscribersForMessageTypes(messageTypes).ToArray();

            if (!subscribers.Any())
            {
                return false;
            }

            var outgoingMessages = BuildOutgoingMessages(correlationId, messages, subscribers);
            PublishMessages(outgoingMessages);

            return true;
        }

        private void PublishMessages(IEnumerable<OutgoingMessage> outgoingMessages)
        {
            if (Settings.IsClientEndpoint)
            {
                messageSender.Send(outgoingMessages);
            }
            else
            {
                var outgoingMessageManager = ServiceLocator.Current.GetService<IProcessOutgoingMessages>();
                outgoingMessageManager.Add(outgoingMessages);
            }
        }

        private IEnumerable<OutgoingMessage> BuildOutgoingMessages(Guid correlationId, object[] messages, IEnumerable<Address> subscribers)
        {
            var outgoingMessages = new List<OutgoingMessage>();

            foreach (var subscriber in subscribers)
            {
                var transportMessage = messageFactory.BuildTransportMessage(correlationId, messages);
                outgoingMessages.Add(new OutgoingMessage(transportMessage, subscriber));
            }

            return outgoingMessages;
        }

        private void GuardPublisher(object[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                throw new InvalidOperationException("Cannot publish an empty set of messages.");
            }

            if (subscriptionStorage == null)
            {
                throw new InvalidOperationException(
                    "Cannot publish on this endpoint - no subscription storage has been configured.");
            }
        }
    }
}
