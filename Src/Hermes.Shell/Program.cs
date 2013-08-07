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
                Console.WriteLine("Selling Shoes");
                Settings.MessageBus.Send(new SellShoes { ShoeTypeId = 1, Size = 2 });
                System.Threading.Thread.Sleep(2000);       
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacAdapter())
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
