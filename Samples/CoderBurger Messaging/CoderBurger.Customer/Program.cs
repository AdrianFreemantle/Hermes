using System;
using System.Reflection;
using CoderBurger.Messages;
using CoderBurger.Messages.Waiter;
using Hermes;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Logging;
using Hermes.Messages;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Transports.SqlServer;
using Stateless;

namespace CoderBurger.Customer
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=CoderBurger;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        static void Main(string[] args)
        {
            Initialize();

            while (true)
            {
                var orderId = Guid.NewGuid();

                Console.WriteLine("Press key to order meal.");
                Console.ReadKey();
                Settings.MessageBus.Send(new PlaceOrder { OrderId = orderId });

                Console.WriteLine("Ordered. Press key to pay for meal.");
                Console.ReadKey();
                Settings.MessageBus.Send(new PayOrder { OrderId = orderId });

                Console.WriteLine("Paid. Press key to collect meal.");
                Console.ReadKey();
                Settings.MessageBus.Send(new CollectOrder { OrderId = orderId });


                Console.ReadKey();
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacServiceAdapter())
                     .ConsoleWindowLogger();

            Configure.Bus(Address.Parse("Customer"))
                     .UsingJsonSerialization()
                     .UsingUnicastBus()
                     .UsingSqlTransport(ConnectionString)
                     .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                     .RegisterMessageRoute<PlaceOrder>(Address.Parse("Waiter"))
                     .RegisterMessageRoute<CancelOrder>(Address.Parse("Waiter"))
                     .RegisterMessageRoute<PayOrder>(Address.Parse("Waiter"))
                     .RegisterMessageRoute<CollectOrder>(Address.Parse("Waiter"))
                     .SubscribeToEvent<OrderReady>()
                     .SubscribeToEvent<CustomerRefunded>()
                     .SubscribeToEvent<OrderCollected>()
                     .Start();
        }
    }

    public class OrderHandlers 
        : IHandleMessage<OrderReady>
        , IHandleMessage<CustomerRefunded>
        , IHandleMessage<OrderCollected>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(OrderHandlers)); 

        public void Handle(OrderReady command)
        {
            Logger.Info("Order {0} is ready", command.OrderId);
        }

        public void Handle(CustomerRefunded command)
        {
            Logger.Info("You have been refunded for order {0}", command.OrderId);
        }

        public void Handle(OrderCollected command)
        {
            Logger.Info("Hmmm... burger and chips", command.OrderId);
        }
    }

    
}
