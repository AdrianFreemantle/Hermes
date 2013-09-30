using System;
using System.Linq;

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

            var transportMessage = messageFactory.BuildTransportMessage(messages);

            foreach (var subscriber in subscribers)
            {
                transportMessage.ChangeMessageId(IdentityFactory.NewComb());
                messageSender.Send(transportMessage, subscriber);
            }

            return true;
        }
    }
}
