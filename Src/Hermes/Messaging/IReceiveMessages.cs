using System;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    /// <summary>
    /// Indicates the ability to receive messages.
    /// </summary>
    /// <remarks>
    /// Object instances which implement this interface should be designed to be single threaded and
    /// should not be shared between threads.
    /// </remarks>
    public interface IReceiveMessages
    {
        /// <summary>
        /// Starts the receipt of messages.
        /// </summary>
        void Start(Action<TransportMessage> handleMessage);

        /// <summary>
        /// Stops receiving new messages.
        /// </summary>
        void Stop();
    }
}