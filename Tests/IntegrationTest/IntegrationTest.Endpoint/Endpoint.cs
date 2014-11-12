using System;
using Hermes.EntityFramework;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using IntegrationTest.Contracts;
using IntegrationTests.PersistenceModel;

namespace IntegrationTest.Endpoint
{
    public class Endpoint : WorkerEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Fatal;

            configuration
                .FlushQueueOnStartup(true)                
                .CircuitBreakerPolicy(100, TimeSpan.FromSeconds(1))
                .FirstLevelRetryPolicy(2)
                .SecondLevelRetryPolicy(10, TimeSpan.FromSeconds(5))
                .DisableHeartbeatService()
                .UseJsonSerialization()
                .UseSqlTransport()
                .DefineCommandAs(IsCommand)
                .RegisterDependencies<DependencyRegistrar>()
                .DefineEventAs(IsEvent)
                .NumberOfWorkers(Environment.ProcessorCount)
                .ConfigureEntityFramework<IntegrationTestContext>("IntegrationTest");
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type);
        }

        private static bool IsEvent(Type type)
        {
            return typeof(IEvent).IsAssignableFrom(type);
        }
    }

    public class DependencyRegistrar : IRegisterDependencies
    {
        public void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<RecordCountWorker>(DependencyLifecycle.SingleInstance);
        }
    }
}