using System;
using Contracts;
using Hermes.EntityFramework;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using LocalBus.Persistence;

namespace LocalBus
{
    public class LocalEndpoint : LocalEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            LogFactory.BuildLogger = t => new ConsoleWindowLogger(t);
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Fatal;

            configuration
                .UseJsonSerialization()
                .DisableHeartbeatService()
                .EnableCommandValidators()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .UseSqlTransport() //we still need the transport so certain dependencies can be resolved and so that we can send async messages
                .ConfigureEntityFramework<LocalBusTestContext>("LocalBusTest"); ;
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
