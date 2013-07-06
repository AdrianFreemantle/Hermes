namespace Hermes.Transports
{
    /// <summary>
    /// Indicates the ability to receive messages.
    /// </summary>
    /// <remarks>
    /// Object instances which implement this interface should be designed to be single threaded and
    /// should not be shared between threads.
    /// </remarks>
    public interface IDequeueMessages
    {
        /// <summary>
        /// Starts the receipt of messages.
        /// </summary>
        void Start(Address queueAddress);

        /// <summary>
        /// Stops receiving new messages.
        /// </summary>
        void Stop();
    }
}