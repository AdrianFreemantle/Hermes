namespace Hermes.Transports
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
        /// Sends the given <paramref name="message"/> to the <paramref name="address"/>.
        /// </summary>
        /// <param name="message"><see cref="TransportMessage"/> to send.</param>
        /// <param name="address">Destination <see cref="Address"/>.</param>
        void Send(MessageEnvelope message, Address address);
    }
}