using System;

using Hermes.Messaging;

namespace Hermes.Transports
{
    /// <summary>
    /// Indicates the ability to transport inbound and outbound messages.
    /// </summary>
    /// <remarks>
    /// Object instances which implement this interface must be designed to be multi-thread safe.
    /// </remarks>
    public interface ITransportMessages : IDisposable
    {
        /// <summary>
        /// Starts the transport listening for new messages to receive.
        /// </summary>
        void Start(Address localAddress);

        /// <summary>
        /// Stops the transport listening for new messages.
        /// </summary>
        void Stop();

        void Send(MessageEnvelope message, Address recipient);
    }
}
