using System;
using System.Reflection;
using CoderBurger.Messages;
using Hermes;
using Hermes.Configuration;
using Hermes.Core;
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

            Console.WriteLine("Waiter Ready");

            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacAdapter())
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
}
