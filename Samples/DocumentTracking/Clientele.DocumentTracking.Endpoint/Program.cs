using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Clientele.DocumentTracking.ApplicationService;
using Clientele.DocumentTracking.DataModel;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.EntityFramework.SagaPersistence;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

namespace Clientele.DocumentTracking.Endpoint
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        static void Main(string[] args)
        {
            CreateDatabaseIfNotExisting();

            //TestError.PercentageFailure = 4;

            var endpoint = Configure
                .Endpoint("DocumentTracking", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .UseEntityFrameworkSagaPersister<DocumentTrackingContext>("DocumentTracking")
                .SecondLevelRetryPolicy(5, TimeSpan.FromSeconds(10))
                .FirstLevelRetryPolicy(0, TimeSpan.FromMilliseconds(0))
                .ScanForHandlersIn(Assembly.Load(new AssemblyName("Clientele.DocumentTracking.OcrService")))
                .SubscribeToEvent<Ocr.Contracts.Events.ExportCompleted>()
                .SubscribeToEvent<Ocr.Contracts.Events.ExportFailed>()
                .SubscribeToEvent<Ocr.Contracts.Events.RecognitionDone>()
                .SubscribeToEvent<Ocr.Contracts.Events.RecognitionFailed>()
                .SubscribeToEvent<Ocr.Contracts.Events.VerificationDone>()
                .NumberOfWorkers(1);

            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;
            Settings.Builder.RegisterType<DocumentTrackingService>(DependencyLifecycle.InstancePerLifetimeScope);

            endpoint.Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(1));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static void CreateDatabaseIfNotExisting()
        {
            using (var context = new DocumentTrackingContext("DocumentTracking"))
            {
                var blah = context.Documents.ToList(); //this will trigger DB creation
            }
        }
    }
}
