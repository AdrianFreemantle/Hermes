using Hermes.Configuration;
using Hermes.Core;
using Hermes.Core.Deferment;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Transports.SqlServer;

namespace Hermes.Tests.Endpoint.Deferment
{
    internal class Program
    {
        private const string connectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=ShoeShop;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            Initialize();

            while (true)
            {
                System.Threading.Thread.Sleep(50);
            }
        }

        private static void Initialize()
        {
            Configure.Environment(new AutofacServiceAdapter())
                     .ConsoleWindowLogger();

            Configure.Bus(Settings.DefermentEndpoint)
                     .UsingJsonSerialization()
                     .UsingDefermentBus(new InMemoryTimeoutPersistence())
                     .UsingSqlTransport(connectionString)
                     .Start();

            var timeoutProcessor = Settings.Builder.GetInstance<ITimeoutProcessor>();
            timeoutProcessor.Start();
        }
    }
}
