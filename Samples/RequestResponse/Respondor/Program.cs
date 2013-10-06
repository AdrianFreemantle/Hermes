using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using RequestResponseMessages;

namespace Respondor
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";

        private static void Main(string[] args)
        {
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            Configure
                .ServerEndpoint("Respondor", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .RegisterMessageRoute<AdditionResult>(Address.Parse("Requestor"))
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(2));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(50);
            }
            
            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }

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
