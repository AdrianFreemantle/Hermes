using System;
using System.Collections.Generic;

using Hermes.Messaging;

namespace Hermes.Storage
{
    public interface IStoreSubscriptions
    {
        /// <summary>
        /// Subscribes the given client address to messages of the given types.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="messageTypes"></param>
        void Subscribe(Address client, params Type[] messageTypes);

        /// <summary>
        /// Unsubscribes the given client address from messages of the given types.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="messageTypes"></param>
        void Unsubscribe(Address client, params Type[] messageTypes);

        /// <summary>
        /// Returns a list of addresses of subscribers that previously requested to be notified
        /// of messages of the given message types.
        /// </summary>
        /// <param name="messageTypes"></param>
        /// <returns></returns>
        IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<Type> messageTypes);
    }
}
