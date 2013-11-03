using System.Collections.Generic;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    /// <summary>
    /// Indicates the ability to send messages.
    /// </summary>
    /// <remarks>
    /// Object instances which implement this interface should be designed to be single threaded and
    /// should not be shared between threads.
    /// </remarks>
    public interface ISendMessages
    {
        /// <summary>
        /// Sends the given <paramref name="transportMessage"/> to the <paramref name="address"/>.
        /// </summary>
        /// <param name="transportMessage"><see cref="TransportMessage"/> to send.</param>
        /// <param name="address">Destination <see cref="Address"/>.</param>
        void Send(TransportMessage transportMessage, Address address);

        void Send(IEnumerable<OutgoingMessage> messages);
    }
}