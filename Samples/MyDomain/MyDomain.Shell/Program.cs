using System;
using System.Reflection;
using System.Threading;

using EventStore;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.Ioc;
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
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MyDomain;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            var contextFactory = new ContextFactory<MyDomainContext>("MyDomain");
            contextFactory.GetContext().Database.CreateIfNotExists();

            var configuration = Configure
                .Endpoint("MyDomain", new AutofacAdapter())
                //.UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .RegisterMessageRoute<IntimateClaimEvent>(Address.Local)
                .RegisterMessageRoute<RegisterClaim>(Address.Local)
                .ScanForHandlersIn(Assembly.Load(new AssemblyName("MyDomain.ApplicationService")), Assembly.Load(new AssemblyName("MyDomain.Persistence.ReadModel")))
                .SubscribeToEvent<ClaimEventIntimated>()
                .SubscribeToEvent<ClaimRegistered>() 
                .NumberOfWorkers(1);

            IStoreEvents eventStore = EventStore.WireupEventStore();

            Settings.Builder.RegisterSingleton<IStoreEvents>(eventStore);
            Settings.Builder.RegisterSingleton<IContextFactory>(contextFactory);
            Settings.Builder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<EventStoreRepository>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<UnitOfWorkManager>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<EventsToPublishUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);


            configuration.Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(10));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
