using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Messaging.Configuration;
using Microsoft.Practices.ServiceLocation;


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

        public bool Publish(IOutgoingMessageContext outgoingMessage)
        {   
            var subscribers = GetMessageSubscribers(outgoingMessage);

            if (!subscribers.Any())
            {
                return false;
            }

            PublishMessages(outgoingMessage, subscribers);

            return true;
        }

        private Address[] GetMessageSubscribers(IOutgoingMessageContext outgoingMessage)
        {
            var messageTypes = outgoingMessage.GetMessageContracts();
            return subscriptionStorage.GetSubscribersForMessageTypes(messageTypes)
                                      .Distinct()
                                      .ToArray();
        }

        private void PublishMessages(IOutgoingMessageContext outgoingMessage, IEnumerable<Address> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                outgoingMessage.SetMessageId(SequentialGuid.New());
                messageSender.Send(outgoingMessage.ToTransportMessage(), subscriber);
            }
        }
    }
}
