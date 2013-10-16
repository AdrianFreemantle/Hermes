using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using Hermes;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

using RequestResponseMessages;

namespace Requestor
{
    class Program
    {
        private static readonly Random Rand = new Random();
        private static ILog Logger;

        private static void Main(string[] args)
        {
            Logger = LogFactory.BuildLogger(typeof(Program));

            using (var requestor = new RequestorEndpoint())
            {
                requestor.Start();

                Console.WriteLine("Press any key to send a request - x to exit");

                while (Console.ReadKey().KeyChar != 'x')
                {
                    Settings.MessageBus.Send(Guid.NewGuid(), NewCalculation()).Register(Completed);
                    Console.WriteLine("Press any key to send a request - x to exit");
                }
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        public static AddNumbers NewCalculation()
        {
            var x = Rand.Next(0, 10);
            var y = Rand.Next(0, 10);
            Logger.Info("Adding numbers {0} and {1}", x, y);
            return new AddNumbers { X = x, Y = y };
        }

        public static void Completed(CompletionResult cr)
        {
            if (cr.ErrorCode == 0)
            {
                var additionResult = (AdditionResult)cr.Messages.FirstOrDefault();
                Logger.Info("Result is {0}", additionResult.Result);
            }
            else
            {
                Logger.Error("Error result returned");
            }
        }
    }
}
