using System;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using RequestResponseMessages;

namespace Requestor
{
    public class RequestorEndpoint : ClientEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            configuration
                .UseJsonSerialization()
                .UseSqlTransport()
                .DefineCommandAs(IsCommand)
                .DefineMessageAs(IsMessage)
                .RegisterMessageRoute<AddNumbers>(Address.Parse("Responder"));
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("RequestResponseMessages");
        }

        private static bool IsMessage(Type type)
        {
            return typeof(IMessage).IsAssignableFrom(type) && type.Namespace.StartsWith("RequestResponseMessages");
        }
    }
}