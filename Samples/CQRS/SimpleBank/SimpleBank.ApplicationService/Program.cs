using System;
using System.Data.Entity;
using System.Reflection;
using System.Threading;

using Hermes.EntityFramework;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.EntityFramework;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using SimpleBank.DataModel;

namespace SimpleBank.ApplicationService
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";

        private static void Main(string[] args)
        {
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            Configure
                .ServerEndpoint("SimpleBank.ApplicationService", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .ConfigureEntityFramework<SimpleBankContext>("SimpleBank")
                //.UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseEntityFrameworkKeyValueStorage()
                .UseSqlStorage(ConnectionString)
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .Start();

            Settings.Builder.RegisterType<AggregateRepository>(DependencyLifecycle.InstancePerLifetimeScope);
            Database.SetInitializer(new SimpleBankContextInitializer());
            var token = new CancellationTokenSource(TimeSpan.FromHours(2));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(50);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
