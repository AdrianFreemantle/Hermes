using System.Threading;
using Hermes.Logging;
using Hermes.Messaging;

using RequestResponseMessages;

namespace Responder
{
    public class AddNumbersHandler : IHandleMessage<AddNumbers>, IHandleMessage<IResultCalculated>
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
            bus.Publish(new ResultCalculated { CalcuationResult = result });
        }

        public void Handle(IResultCalculated message)
        {
            logger.Info("Event Result is {0}", message.CalcuationResult);
        }
    }
}