using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Logging;
using Hermes.Messaging;
using RequestResponseMessages;

namespace Requestor
{
    class Program
    {
        private static readonly Random Rand = new Random();
        private static ILog logger;

        private static void Main(string[] args)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);

            using (var requestor = new RequestorEndpoint())
            {
                logger = LogFactory.BuildLogger(typeof(Program));
                requestor.Start();
                Console.ReadKey();
                SendRequests(requestor.MessageBus);
            }
        }

        private static void SendRequests(IMessageBus messageBus)
        {
            while (true)
            {
                var x = Rand.Next(0, 10);
                var y = Rand.Next(0, 10);

                logger.Info("Adding numbers {0} and {1}", x, y);
                messageBus.Send(new AddNumbers { X = x, Y = y }).Register().Wait();// wait for response
                Thread.Sleep(50); //small sleep gives the handler a chance to print its result before we send the next command
                Console.ReadKey();
            }
        }
    }

    public class ResultHandler : IHandleMessage<AdditionResult>
    {
        readonly ILog logger = LogFactory.BuildLogger(typeof(ResultHandler));

        public void Handle(AdditionResult message)
        {
            logger.Info("Result is {0}", message.CalcuationResult);
        }
    }
}
