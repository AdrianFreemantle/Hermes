using System;
using Hermes.EntityFramework;
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
                .FirstLevelRetryPolicy(0)
                .SecondLevelRetryPolicy(100, TimeSpan.FromSeconds(5))
                .UseJsonSerialization()
                .UseSqlTransport()
                .DefineCommandAs(IsCommand)
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
}