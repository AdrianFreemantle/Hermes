﻿using System;
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

            Console.WriteLine("BurgerCheff Ready");

            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacAdapter())
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

    public class BurgerCheffHandler : IHandleMessage<OrderPlaced>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(BurgerCheffHandler));
        private readonly IMessageBus messageBus;
        private readonly Random rand = new Random((int)DateTime.Now.Ticks);

        public BurgerCheffHandler(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Handle(OrderPlaced command)
        {
            Logger.Info("Received notice of new order");
            System.Threading.Thread.Sleep(rand.Next(1000, 5000));
            Logger.Info("Order processed");

            messageBus.Publish(new BurgerPrepared() { OrderId = command.OrderId });
        }
    }
}
