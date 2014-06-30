using System;
using Contracts;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

namespace Remote.Endpoint
{
    public class Endpoint : LocalEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            LogFactory.BuildLogger = t => new ConsoleWindowLogger(t);
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            configuration
                .UseJsonSerialization()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .UseSqlTransport(); //we still need the transport so certain dependencies can be resolved and so that we can send async messages
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
