using System;
using System.Reflection;
using System.Threading;

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
using MyDomain.Domain.Events;
using MyDomain.Infrastructure;
using MyDomain.Infrastructure.EntityFramework;
using MyDomain.Persistence.ReadModel;

namespace MyDomain.Shell
{
    class Program
    {
        private static ILog Logger;
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            Logger = LogFactory.BuildLogger(typeof(Program));

            var contextFactory = new ContextFactory<MyDomainContext>("MyDomain");
            contextFactory.GetContext().Database.CreateIfNotExists();

            Configure.Environment(new AutofacAdapter());

            Settings.Builder.RegisterSingleton<IContextFactory>(contextFactory);
            Settings.Builder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<EventStoreRepository>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<UnitOfWorkManager>(DependencyLifecycle.InstancePerLifetimeScope);

            Configure
                .Bus(Address.Parse("MyDomain"))
                .UsingJsonSerialization()
                .UsingUnicastBus()
                .UseDistributedTransaction()
                .UsingSqlTransport(ConnectionString)
                .UsingSqlStorage(ConnectionString)
                .RegisterMessageRoute<IntimateClaimEvent>(Settings.ThisEndpoint)
                .RegisterMessageRoute<RegisterClaim>(Settings.ThisEndpoint)
                .ScanForHandlersIn(Assembly.Load(new AssemblyName("MyDomain.ApplicationService")), Assembly.Load(new AssemblyName("MyDomain.Persistence.ReadModel")))
                .SubscribeToEvent<ClaimEventIntimated>()
                .SubscribeToEvent<ClaimRegistered>() 
                .NumberOfWorkers(1)
                .Start();

            Settings.Builder.RegisterSingleton<IStoreEvents>(EventStore.WireupEventStore());
     
            var token = new CancellationTokenSource(TimeSpan.FromHours(1));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
