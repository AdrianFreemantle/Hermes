using System;
using System.Reflection;
using System.Threading;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using MyDomain.ApplicationService;
using MyDomain.Infrastructure;
using MyDomain.Infrastructure.EntityFramework;
using MyDomain.Persistence.ReadModel;
using MyDomain.Shell;

namespace MyDomain.Producer
{
    class Program
    {
        private static ILog Logger;

        private const string ConnectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=MyDomain;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

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
                .RegisterMessageRoute<IntimateClaimEvent>(Address.Parse("MyDomain"))
                .RegisterMessageRoute<RegisterClaim>(Address.Parse("MyDomain"));

            Logger = LogFactory.BuildLogger(typeof(Program));

            configuration.Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(10));

            var claimEventId = Guid.NewGuid();

            Settings.MessageBus.Send(new IntimateClaimEvent
            {
                Id = claimEventId,
                MessageId = Guid.NewGuid()
            });


            while (!token.IsCancellationRequested)
            {
                try
                {
                    var command = new RegisterClaim
                    {
                        Amount = 10,
                        ClaimEventId = claimEventId,
                        ClaimId = Guid.NewGuid()
                    };

                    Logger.Info("Register Claim {0}", command.ClaimId);

                    Settings.MessageBus.Send(command.ClaimId, command);
                    Thread.Sleep(20);
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
