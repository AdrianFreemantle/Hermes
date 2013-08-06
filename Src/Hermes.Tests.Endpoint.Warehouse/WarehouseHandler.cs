using System;

using Hermes.Messages;
using Hermes.Tests.Messages;

namespace Hermes.Tests.Endpoint.Warehouse
{
    public class WarehouseHandler : IHandleMessage<ShoesSold>
    {
        private readonly IMessageBus messageBus;

        public WarehouseHandler(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Handle(ShoesSold command)
        {
            Console.WriteLine("Publishing order shipped event");

            messageBus.Publish(new OrderShipped { OrderNumber = command.OrderNumber });
        }
    }
}