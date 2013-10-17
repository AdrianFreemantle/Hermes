using System;
using Hermes.Logging;
using Hermes.Messaging;
using Starbucks.Messages;

namespace Starbucks.Barrista
{
    public class Barrista : IHandleMessage<BuyCoffee>
    {
        private readonly IMessageBus bus;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Barrista));

        public Barrista(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(BuyCoffee message)
        {
            Logger.Info("Barista is attempting to prepare your order");
            System.Threading.Thread.Sleep(2000);

            if (DateTime.Now.Ticks % 2 == 0)
            {
                throw new Exception("Blah");     
            }
            
            if (DateTime.Now.Ticks % 5 == 0)
            {
                Logger.Info("Out of coffee!");
                bus.Return(ErrorCodes.Error);
            }
            else
            {
                Logger.Info("Barista is completed your order");
                bus.Return(ErrorCodes.Success);         
            }
        }
    }
}