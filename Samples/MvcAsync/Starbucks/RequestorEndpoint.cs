using System;
using System.Collections.Generic;
using System.Data.Entity;

using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Persistence;
using Hermes.Serialization.Json;
using Hermes.Transports.SqlServer;
using Hermes.EntityFramework;
using Starbucks.Messages;

namespace Starbucks
{
    public class Blah : DbContext
    {
        
    }

    public class RequestorEndpoint : ClientEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .ConfigureEntityFramework<Blah>()
                .UseJsonSerialization()
                .UseSqlTransport()
                .UseSqlStorage()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .SubscribeToEvent<IDrinkPrepared>()
                .SubscribeToEvent<IOrderReady>()
                .RegisterMessageRoute<PlaceOrder>(Address.Parse("Starbucks.Barrista"));
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
        public BarristaEventHandlers(EntityFrameworkUnitOfWork unitOfWork)
        {
             Console.WriteLine("Blah");
        }

        public void Handle(IDrinkPrepared message)
        {
            System.Diagnostics.Trace.WriteLine(message.Drink);    
        }

        public void Handle(IOrderReady message)
        {
            System.Diagnostics.Trace.WriteLine(message.OrderNumber);   
        }
    }

    public class BarristaEventHandlers2
       : IHandleMessage<IDrinkPrepared>
       , IHandleMessage<IOrderReady>
    {
        public BarristaEventHandlers2(IRepositoryFactory repositoryFactory)
        {
            Console.WriteLine("Blah");
        }

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