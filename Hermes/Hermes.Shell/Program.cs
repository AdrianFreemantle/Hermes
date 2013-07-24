using System;
using System.Reflection;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Tests.Messages;
using Hermes.Transports.SqlServer;

namespace Hermes.Shell
{
    class Program
    {
        private const string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=ShoeShop;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        static void Main(string[] args)
        {
            Initialize();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);       
                Console.WriteLine("Selling Shoes");
                Settings.MessageBus.Send(new SellShoes { ShoeTypeId = 1, Size = 2 });
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacServiceAdapter())
                    .ConsoleWindowLogger();

            Configure.Bus(Address.Parse("Shell"))
                     .UsingJsonSerialization()
                     .UsingUnicastBus()
                     .UsingSqlTransport(connectionString)
                     .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                     .RegisterMessageRoute<SellShoes>(Address.Parse("Sales"))
                     .SubscribeToEvent<OrderShipped>()
                     .Start();
        }
    }
}
