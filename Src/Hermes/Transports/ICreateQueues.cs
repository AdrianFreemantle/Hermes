using Hermes.Messaging;

namespace Hermes.Transports
{
    /// <summary>
    /// Abstraction of the capability to create queues
    /// </summary>
    public interface ICreateQueues
    {
        /// <summary>
        /// Create a messages queue where its name is the address parameter, for the given account.
        /// </summary>
        /// <param name="address"></param>
        void CreateQueueIfNecessary(Address address);
    }
}