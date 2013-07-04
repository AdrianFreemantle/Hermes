using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// Forcefully aborts the receipt of new messages and kills all workers currently processing messages.
        /// </summary>
        void Abort();

        /// <summary>
        /// Sends the message provided to the set of receipients indicated.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="recipients">The set of addresses for the interested recipients.</param>
        void Send(MessageEnvelope message, params Address[] recipients);

        /// <summary>
        /// Gets the maximum concurrency level this <see cref="ITransportMessages"/> is able to support.
        /// </summary>
        int MaximumConcurrencyLevel { get; }

        /// <summary>
        /// Updates the maximum concurrency level this <see cref="ITransportMessages"/> is able to support.
        /// </summary>
        /// <param name="maximumConcurrencyLevel">The new maximum concurrency level for this <see cref="ITransportMessages"/>.</param>
        void ChangeMaximumConcurrencyLevel(int maximumConcurrencyLevel);
    }
}
