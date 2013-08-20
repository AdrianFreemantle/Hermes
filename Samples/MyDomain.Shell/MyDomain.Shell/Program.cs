using System;
using System.Reflection;
using System.Threading;
using EventStore;
using Hermes;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
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
                .RegisterMessageRoute<IntimateClaimEvent>(Settings.ThisEndpoint)
                .RegisterMessageRoute<RegisterClaim>(Settings.ThisEndpoint)
                .ScanForHandlersIn(Assembly.Load(new AssemblyName("MyDomain.ApplicationService")), Assembly.Load(new AssemblyName("MyDomain.Persistence.ReadModel")))
                .SubscribeToEvent<ClaimEventIntimated>()
                .SubscribeToEvent<ClaimRegistered>() 
                .NumberOfWorkers(1)
                .Start();

            Settings.Builder.RegisterSingleton<IStoreEvents>(EventStore.WireupEventStore());
     
            var token = new CancellationTokenSource(TimeSpan.FromMinutes(10));

            while (!token.IsCancellationRequested)
            {
                Logger.Info("=================================================");         
                Settings.MessageBus.Send(new IntimateClaimEvent { Id = Guid.NewGuid(), MessageId = Guid.NewGuid() });
                Thread.Sleep(1000);
                //Console.ReadKey();
            }

            Console.WriteLine("Finished");
        }
    }
}
