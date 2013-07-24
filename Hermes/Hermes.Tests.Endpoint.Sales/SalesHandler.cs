using System;

using Hermes.Messages;
using Hermes.Tests.Messages;

namespace Hermes.Tests.Endpoint.Sales
{
    public class SalesHandler : IHandleMessage<SellShoes>
    {
        private readonly IMessageBus bus;

        public SalesHandler(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(SellShoes command)
        {
            Console.WriteLine("Publishing shoes sold event");

            bus.Defer(TimeSpan.FromSeconds(10), new ShoesSold
            {
                OrderNumber = DateTime.Now.Ticks.ToString(),
                ShoeTypeId = command.ShoeTypeId,
                Size = command.Size
            });
        }
    }
}