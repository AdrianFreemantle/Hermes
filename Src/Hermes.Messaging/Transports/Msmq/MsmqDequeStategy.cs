using System;

namespace Hermes.Messaging.Transports.Msmq
{
    public class MsmqDequeStategy : IDequeueMessages
    {
        public TransportMessage Dequeue()
        {
            throw new NotImplementedException();
        }
    }

    public class MsmQMessageSender : ISendMessages
    {
        public void Send(TransportMessage transportMessage, Address address)
        {
            throw new NotImplementedException();
        }
    }

    public class MsmqQueueCreator : ICreateMessageQueues
    {
        public void CreateQueueIfNecessary(Address address)
        {
            throw new NotImplementedException();
        }

        public void Purge(Address address)
        {
            throw new NotImplementedException();
        }
    }
}
