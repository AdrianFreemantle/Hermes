using System;
using System.Runtime.Serialization;
using Hermes;
using Hermes.Logging;
using Hermes.Messaging;
using Starbucks.Messages;

namespace Starbucks.Barrista
{
    public class Barrista : IHandleMessage<OrderCoffee>
    {
        static readonly Random rand = new Random(DateTime.Now.Second);

        private readonly IMessageBus bus;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Barrista));

        public Barrista(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(OrderCoffee message)
        {
            Logger.Info("Barista is attempting to prepare order");

            //var secondsToSleep = rand.Next(1, 4);
            //System.Threading.Thread.Sleep(secondsToSleep * 1000);

            if (DateTime.Now.Ticks % 5568645674894681658 == 0)
            {
                Logger.Info("Out of coffee!");
                bus.Return(ErrorCodes.OutOfCoffee);
            }
            else
            {
                Logger.Info("Barista has completed order");
                bus.Return(ErrorCodes.Success);
                bus.Publish(new CoffeeReady(message.OrderNumber, message.Coffee));
            }
        }
    }    
}