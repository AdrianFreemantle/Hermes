using System;
using System.Reflection;
using System.Threading;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;
using MyDomain.ApplicationService.Commands;

namespace MyDomain.Producer
{
    class Program
    {
        private static ILog Logger;

        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            var configuration = Configure.Endpoint("Producer", new AutofacAdapter())
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseConsoleWindowLogger()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .RegisterMessageRoute<IntimateClaim>(Address.Parse("MyDomain"))
                .RegisterMessageRoute<CloseClaim>(Address.Parse("MyDomain"))
                .RegisterMessageRoute<OpenClaim>(Address.Parse("MyDomain"));

            Logger = LogFactory.BuildLogger(typeof(Program));

            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            configuration.Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(1));


            while (!token.IsCancellationRequested)
            {
                try
                {
                    var claimEventId = Guid.NewGuid();
                    
                    var intimateCommand = new IntimateClaim
                    {
                        ClaimEventId = DateTime.Now.Ticks.ToString().Substring(0, 6)
                    };

                    var closeClaimCommand = new CloseClaim
                    {
                        ClaimEventId = claimEventId
                    };

                    var openClaimCommand = new OpenClaim
                    {
                        ClaimEventId = claimEventId
                    };

                    Settings.MessageBus.Send(intimateCommand);
                    Settings.MessageBus.Send(closeClaimCommand);
                    Settings.MessageBus.Send(openClaimCommand);

                    Logger.Info("Sent intimate command for claim event {0}", claimEventId);

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error {0}.", ex);
                }

                
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
