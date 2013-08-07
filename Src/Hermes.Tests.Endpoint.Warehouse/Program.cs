using System.Reflection;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Tests.Messages;
using Hermes.Transports.SqlServer;

namespace Hermes.Tests.Endpoint.Warehouse
{
    class Program
    {
        private const string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=ShoeShop;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        static void Main(string[] args)
        {
            Initialize();

            while (true)
            {
                System.Threading.Thread.Sleep(50);
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacAdapter())
                     .ConsoleWindowLogger();

            Configure.Bus(Address.Parse("Warehouse"))
                     .UsingJsonSerialization()
                     .UsingUnicastBus()
                     .UsingSqlTransport(connectionString)
                     .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                     .RegisterMessageRoute<OrderShipped>(Address.Parse("Shell"))
                     .SubscribeToEvent<ShoesSold>()
                     .Start();
        }
    }
}
