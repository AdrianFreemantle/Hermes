using System;
using System.Collections.Generic;
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

namespace CoderBurger.Waiter
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=CoderBurger;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        static void Main(string[] args)
        {
            Initialize();

            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacServiceAdapter())
                     .ConsoleWindowLogger();

            Configure.Bus(Address.Parse("Waiter"))
                     .UsingJsonSerialization()
                     .UsingUnicastBus()
                     .UsingSqlTransport(ConnectionString)
                     .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                     .RegisterMessageRoute<AbandonOrder>(Settings.ThisEndpoint)
                     .RegisterMessageRoute<CancelOrder>(Settings.ThisEndpoint)
                     .RegisterMessageRoute<RefundCustomer>(Settings.ThisEndpoint)
                     .SubscribeToEvent<FriesPrepared>()
                     .SubscribeToEvent<BurgerPrepared>()
                     .SubscribeToEvent<DrinkPrepared>()
                     .Start();
        }
    }

    public static class OrderWorkflowStore
    {
        public static Dictionary<Guid, OrderWorkflow> Store { get; set; } 

        static OrderWorkflowStore()
        {
            Store = new Dictionary<Guid, OrderWorkflow>();
        }
    }

    public class WaiterHandler 
        : IHandleMessage<AbandonOrder>
        , IHandleMessage<PayOrder>
        , IHandleMessage<CancelOrder>
        , IHandleMessage<CollectOrder>
        , IHandleMessage<PlaceOrder>
        , IHandleMessage<RefundCustomer>
        , IHandleMessage<FriesPrepared>
        , IHandleMessage<BurgerPrepared>
        , IHandleMessage<DrinkPrepared>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(WaiterHandler));
        private readonly IMessageBus messageBus;

        public WaiterHandler(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Handle(PayOrder command)
        {
            Logger.Info("Accepting payment for order {0}", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.Pay();
        }

        public void Handle(AbandonOrder command)
        {
            Logger.Info("Abandoning order {0}", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.Abandon();
        }

        public void Handle(CancelOrder command)
        {
            Logger.Info("Cancelling order {0}", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.CancelOrder();
        }

        public void Handle(CollectOrder command)
        {
            Logger.Info("Order {0} is being collected", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.Collect();
        }

        public void Handle(PlaceOrder command)
        {
            Logger.Info("Accpeting new order {0}", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.PlaceOrder();
        }

        public void Handle(RefundCustomer command)
        {
            Logger.Info("Refunding customer for order {0}", command.OrderId);
            messageBus.Publish(new CustomerRefunded { OrderId = command.OrderId });
        }

        public void Handle(FriesPrepared command)
        {
            Logger.Info("Recieved fries for order {0}", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.FriesPrepared();
        }

        public void Handle(BurgerPrepared command)
        {
            Logger.Info("Recieved burger for order {0}", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.BurgerPrepared();
        }

        public void Handle(DrinkPrepared command)
        {
            Logger.Info("Recieved drink for order {0}", command.OrderId);
            var workflow = GetWorkflow(command.OrderId);
            workflow.DrinkPrepared();
        }

        private OrderWorkflow GetWorkflow(Guid orderId)
        {
            if (!OrderWorkflowStore.Store.ContainsKey(orderId))
            {
                OrderWorkflowStore.Store[orderId] = new OrderWorkflow(messageBus, orderId);
            }

            return OrderWorkflowStore.Store[orderId];
        }
    }
}
