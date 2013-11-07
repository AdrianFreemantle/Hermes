using System;

using Hermes;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Starbucks.Messages;

namespace Starbucks.Barrista
{
    public class BarristaEndpoint : WorkerEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            configuration
                .SecondLevelRetryPolicy(3, TimeSpan.FromSeconds(10))
                .FirstLevelRetryPolicy(1, TimeSpan.FromMilliseconds(10))
                .UseJsonSerialization()
                .UseSqlTransport()
                .UseSqlStorage()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .NumberOfWorkers(5);
        }

        private static bool IsCommand(Type type)
        {
            return typeof (ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }

        private static bool IsEvent(Type type)
        {
            return typeof(IEvent).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }
    }
}
