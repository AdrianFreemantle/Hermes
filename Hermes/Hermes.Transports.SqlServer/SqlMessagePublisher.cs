using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Subscriptions;

namespace Hermes.Transports.SqlServer
{
    /// <summary>
    /// Published messages based on whats registered in the given subscription storage
    /// </summary>
    public class SqlMessagePublisher : IPublishMessages
    {
        private readonly ISubscriptionStorage subscriptionStorage;
        private readonly ISendMessages messageSender;

        public SqlMessagePublisher(ISendMessages messageSender, ISubscriptionStorage subscriptionStorage)
        {
            this.messageSender = messageSender;
            this.subscriptionStorage = subscriptionStorage;
        }

        /// <summary>
        /// Pubvlishes the given message to all subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventTypes"></param>
        /// <returns></returns>
        public bool Publish(MessageEnvelope message, IEnumerable<Type> eventTypes)
        {
            if (subscriptionStorage == null)
                throw new InvalidOperationException("Cannot publish on this endpoint - no subscription storage has been configured. Add either 'MsmqSubscriptionStorage()' or 'DbSubscriptionStorage()' after 'NServiceBus.Configure.With()'.");

            var subscribers = subscriptionStorage.GetSubscriberAddressesForMessage(eventTypes).ToList();

            if (!subscribers.Any())
            {
                return false;
            }

            foreach (var subscriber in subscribers)
            {
                //this is unicast so we give the message a unique ID
                message.ChangeMessageId(IdentityFactory.NewComb());
                messageSender.Send(message, subscriber);
            }

            return true;
        }
    }
}
