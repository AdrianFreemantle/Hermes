using System;

using Hermes.Logging;
using Hermes.Messaging;

using RequestResponseMessages;

namespace Responder
{
    public class AddNumbersHandler : IHandleMessage<AddNumbers>
    {
        private readonly IMessageBus bus;
        readonly ILog Logger = LogFactory.BuildLogger(typeof(AddNumbersHandler));

        public AddNumbersHandler(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(AddNumbers message)
        {
            if (DateTime.Now.Ticks % 2 == 0)
            {
                var result = message.X + message.Y;

                Logger.Info("{0} = {1} + {2}", result, message.X, message.Y);
                bus.Reply(new AdditionResult { Result = result });
            }
            else
            {
                Logger.Warn("Simulating response to failed business rule.");
                bus.Return(ErrorCodes.CalculationFault);
            }
        }
    }
}