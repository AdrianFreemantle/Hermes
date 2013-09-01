using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using RequestResponseMessages;

namespace Respondor
{
    class Program
    {
        private static ILog Logger;
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            Configure
                .Endpoint("Respondor", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .RegisterMessageRoute<AdditionResult>(Address.Parse("Requestor"))
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .Start();

            Logger = LogFactory.BuildLogger(typeof(Program));
            var token = new CancellationTokenSource(TimeSpan.FromHours(1));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(1000);
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
                Logger.Info("Adding numbers {0} and {1}", message.X, message.Y);
                bus.Reply(new AdditionResult {Result = message.X + message.Y});
            }
            else
            {
                Logger.Info("Sending error response");
                bus.Return(ErrorCodes.CalculationFault);
            }
        }
    }
}
