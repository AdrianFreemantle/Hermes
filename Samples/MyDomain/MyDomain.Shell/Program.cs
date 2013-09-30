using System;
using System.Data.Entity;
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
using MyDomain.ApplicationService.Commands;
using MyDomain.Infrastructure;
using MyDomain.Infrastructure.EntityFramework;
using MyDomain.Persistence.ReadModel;
using NEventStore;

namespace MyDomain.Shell
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            var contextFactory = new ContextFactory<MyDomainContext>("MyDomain");
            DbContext context = contextFactory.GetContext();
            
            //context.Database.CreateIfNotExists();
            //context.Dispose();

            var configuration = Configure
                .Endpoint("MyDomain", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .SecondLevelRetryPolicy(5, TimeSpan.FromSeconds(5))
                .RegisterMessageRoute<IntimateClaim>(Address.Local)
                .ScanForHandlersIn(Assembly.Load(new AssemblyName("MyDomain.ApplicationService")))
                .NumberOfWorkers(1);

            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            IStoreEvents eventStore = EventStore.WireupEventStore();
            Settings.Builder.RegisterSingleton<IStoreEvents>(eventStore);
            Settings.Builder.RegisterType<EventStoreRepository>(DependencyLifecycle.InstancePerLifetimeScope);

            configuration.Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(1));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
