using System.Collections.Generic;
using System.Linq;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Bus
{
    public class StorageDrivenPublisher : IPublishMessages
    {
        private readonly IStoreSubscriptions subscriptionStorage;
        private readonly ISendMessages messageSender;

        public StorageDrivenPublisher(ISendMessages messageSender, IStoreSubscriptions subscriptionStorage)
        {
            this.messageSender = messageSender;
            this.subscriptionStorage = subscriptionStorage;
        }

        public bool Publish(OutgoingMessageContext outgoingMessage)
        {   
            var subscribers = GetMessageSubscribers(outgoingMessage);

            if (!subscribers.Any())
            {
                return false;
            }

            PublishMessages(outgoingMessage, subscribers);

            return true;
        }

        private Address[] GetMessageSubscribers(OutgoingMessageContext outgoingMessage)
        {
            var messageTypes = outgoingMessage.GetMessageContracts();
            return subscriptionStorage.GetSubscribersForMessageTypes(messageTypes)
                                      .Distinct()
                                      .ToArray();
        }

        private void PublishMessages(OutgoingMessageContext outgoingMessage, IEnumerable<Address> subscribers)
        {
            TransportMessage message = outgoingMessage.GetTransportMessage();

            foreach (var subscriber in subscribers)
            {
                message.SetMessageId(SequentialGuid.New());
                messageSender.Send(message, subscriber);
            }
        }
    }
}
