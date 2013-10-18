using System.Threading;
using Hermes.Logging;
using Hermes.Messaging;

using RequestResponseMessages;

namespace Responder
{
    public class AddNumbersHandler : IHandleMessage<AddNumbers>
    {
        private readonly IMessageBus bus;
        readonly ILog logger = LogFactory.BuildLogger(typeof(AddNumbersHandler));

        public AddNumbersHandler(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(AddNumbers message)
        {
            int result = message.X + message.Y;

            logger.Info("{0} = {1} + {2}", result, message.X, message.Y);
            Thread.Sleep(1000);
         
            bus.Reply(new AdditionResult { CalcuationResult = result });
        }
    }
}