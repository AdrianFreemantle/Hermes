using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Storage;
using Hermes.Transports;

namespace Hermes.Core
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
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot publish an empty set of messages.");

            if (subscriptionStorage == null)
                throw new InvalidOperationException("Cannot publish on this endpoint - no subscription storage has been configured.");

            var messageTypes = messages.Select(o => o.GetType());
            var subscribers = subscriptionStorage.GetSubscriberAddressesForMessage(messageTypes).ToList();

            if (!subscribers.Any())
            {
                return false;
            }

            var outgoingMessages = new List<OutgoingMessage>();

            foreach (var subscriber in subscribers)
            {
                var transportMessage = messageFactory.BuildTransportMessage(messages);
                outgoingMessages.Add(new OutgoingMessage(transportMessage, subscriber));
            }

            if (Settings.IsSendOnlyEndpoint)
            {
                messageSender.Send(outgoingMessages);
            }
            else
            {
                var outgoingMessageManager = ServiceLocator.Current.GetService<IManageOutgoingMessages>();
                outgoingMessageManager.Add(outgoingMessages);
            }

            return true;
        }
    }
}
