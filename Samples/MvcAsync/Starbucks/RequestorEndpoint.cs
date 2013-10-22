using System;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Transports.SqlServer;
using Starbucks.Messages;

namespace Starbucks
{
    public class RequestorEndpoint : ClientEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .UseJsonSerialization()
                .UseSqlTransport()
                .UseSqlStorage()
                .DefineCommandAs(IsCommand)
                .RegisterMessageRoute<OrderCoffee>(Address.Parse("Starbucks.Barrista"));
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }
    }
}