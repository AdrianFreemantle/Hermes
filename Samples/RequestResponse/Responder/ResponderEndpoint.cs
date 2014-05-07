using System;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

using RequestResponseMessages;

namespace Responder
{
    public class ResponderEndpoint : WorkerEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            configuration
                .FirstLevelRetryPolicy(3)
                .UseJsonSerialization()
                .UseSqlTransport()
                .DefineMessageAs(IsMessage)
                .DefineCommandAs(IsCommand)
                .RegisterMessageRoute<AdditionResult>(Address.Parse("Requestor"));
        }

        private static bool IsMessage(Type type)
        {
            return typeof(IMessage).IsAssignableFrom(type) && type.Namespace.StartsWith("RequestResponseMessages");
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("RequestResponseMessages");
        }
    }
}
