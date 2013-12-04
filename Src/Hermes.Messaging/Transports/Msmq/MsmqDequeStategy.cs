using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Messaging.Transports.Msmq
{
    public class MsmqDequeStategy : IDequeueMessages
    {
        public TransportMessage Dequeue(Address address)
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

    public class MsmqQueueCreator : ICreateQueues
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
