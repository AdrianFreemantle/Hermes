namespace Hermes.Messaging
{
    /// <summary>
    /// Abstraction of the capability to create queues
    /// </summary>
    public interface ICreateMessageQueues
    {
        /// <summary>
        /// Create a messages queue where its name is the address parameter, for the given account.
        /// </summary>
        /// <param name="address"></param>
        void CreateQueueIfNecessary(Address address);
        void Purge(Address address);
    }
}