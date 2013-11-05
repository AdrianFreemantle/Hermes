using System;

using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Messaging.Bus.Transports.SqlTransport;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

using Starbucks.Barrista.Wcf.Queries;
using Starbucks.Messages;

namespace Starbucks.Barrista.Wcf
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
                .RegisterDependencies(new QueryDependencies())
                .RegisterMessageRoute<PlaceOrder>(Address.Parse("Starbucks.Barrista"));
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }
    }

    public class QueryDependencies : IRegisterDependencies
    {
        public void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<OrderStatusQueryHandler>(DependencyLifecycle.InstancePerDependency);
        }
    }
}