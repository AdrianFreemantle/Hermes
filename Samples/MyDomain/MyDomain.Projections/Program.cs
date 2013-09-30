using System;
using System.Reflection;
using System.Threading;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using MyDomain.Domain.Events;
using MyDomain.Infrastructure.EntityFramework;
using MyDomain.Persistence.ReadModel;

namespace MyDomain.Projections
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            TestError.PercentageFailure = 5;

            var configuration = Configure
                .Endpoint("Projections", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .SecondLevelRetryPolicy(20, TimeSpan.FromSeconds(2))
                .FirstLevelRetryPolicy(0, TimeSpan.FromSeconds(0))
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .SubscribeToEvent<ClaimEventIntimated>()
                .SubscribeToEvent<ClaimEventClosed>()
                .SubscribeToEvent<ClaimEventOpened>()
                .NumberOfWorkers(2);

            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            Settings.Builder.RegisterSingleton<IContextFactory>(new ContextFactory<MyDomainContext>("MyDomain"));
            Settings.Builder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<UnitOfWorkManager>(DependencyLifecycle.InstancePerLifetimeScope);

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
