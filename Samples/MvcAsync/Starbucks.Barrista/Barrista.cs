using System;

using Hermes.Logging;
using Hermes.Messaging;
using Starbucks.Messages;

namespace Starbucks.Barrista
{
    public class Barrista : IHandleMessage<PlaceOrder>
    {
        private readonly IMessageBus bus;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Barrista));

        public Barrista(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(PlaceOrder message)
        {
            Logger.Info("Barista is attempting to prepare order");

            if (DateTime.Now.Ticks % 2 == 0)
            {
                Logger.Info("Out of coffee!");
                bus.Return(ErrorCodes.OutOfCoffee);
            }
            else
            {
                Logger.Info("Barista has completed order");
                bus.Return(ErrorCodes.Success);
            }
        }
    }    
}