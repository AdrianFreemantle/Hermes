using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoderBurger.Messages;
using Hermes;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Logging;
using Hermes.Messages;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Transports.SqlServer;

namespace CoderBurger.BurgerCheff
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

            Configure.Bus(Address.Parse("BurgerCheff"))
                     .UsingJsonSerialization()
                     .UsingUnicastBus()
                     .UsingSqlTransport(ConnectionString)
                     .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                     .SubscribeToEvent<OrderPlaced>()
                     .Start();
        }
    }

    public class WaiterHandler : IHandleMessage<OrderPlaced>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(WaiterHandler));
        private readonly IMessageBus messageBus;

        public WaiterHandler(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Handle(OrderPlaced command)
        {
            Logger.Info("Received notice of new order {0}", command.OrderId);
            messageBus.Publish(new BurgerPrepared() { OrderId = command.OrderId });
        }
    }
}
