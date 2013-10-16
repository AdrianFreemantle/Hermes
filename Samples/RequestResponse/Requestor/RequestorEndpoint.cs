using System;

using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using RequestResponseMessages;

namespace Requestor
{
    public class RequestorEndpoint : ClientEndpoint<AutofacAdapter>
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";

        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .UseJsonSerialization()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
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