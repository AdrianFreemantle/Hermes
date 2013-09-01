using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EventStore;

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
            @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            Logger = LogFactory.BuildLogger(typeof (Program));

            var contextFactory = new ContextFactory<MyDomainContext>("MyDomain");
            contextFactory.GetContext().Database.CreateIfNotExists();

            Configure.Environment(new AutofacAdapter());

            Settings.Builder.RegisterSingleton<IContextFactory>(contextFactory);
            Settings.Builder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<EventStoreRepository>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<UnitOfWorkManager>(DependencyLifecycle.InstancePerLifetimeScope);

            Configure
                .Bus(Address.Parse("Producer"))
                .UsingJsonSerialization()
                .UsingUnicastBus()
                .UseDistributedTransaction()
                .UsingSqlTransport(ConnectionString)
                .UsingSqlStorage(ConnectionString)
                .RegisterMessageRoute<IntimateClaimEvent>(Address.Parse("MyDomain"))
                .RegisterMessageRoute<RegisterClaim>(Address.Parse("MyDomain"))
                .Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(1));


            while (!token.IsCancellationRequested)
            {
                Logger.Info("=================================================");
                var claimEventId = Guid.NewGuid();

                try
                {
                    Settings.MessageBus.Send(new IntimateClaimEvent {Id = claimEventId, MessageId = Guid.NewGuid()},
                        new RegisterClaim {Amount = 10, ClaimEventId = claimEventId, ClaimId = Guid.NewGuid()});

                    //Console.ReadKey();
                    Thread.Sleep(500);
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
