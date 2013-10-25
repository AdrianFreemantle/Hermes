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
                .DefineEventAs(IsEvent)
                .SubscribeToEvent<IDrinkPrepared>()
                .SubscribeToEvent<IOrderReady>()
                .RegisterMessageRoute<OrderCoffee>(Address.Parse("Starbucks.Barrista"));
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }

        private static bool IsEvent(Type type)
        {
            return typeof(IEvent).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }
    }

    public class BarristaEventHandlers 
        : IHandleMessage<IDrinkPrepared>
        , IHandleMessage<IOrderReady>
    {
        
        public void Handle(IDrinkPrepared message)
        {
            System.Diagnostics.Trace.WriteLine(message.Drink);    
        }

        public void Handle(IOrderReady message)
        {
            System.Diagnostics.Trace.WriteLine(message.OrderNumber);   
        }
    }
}