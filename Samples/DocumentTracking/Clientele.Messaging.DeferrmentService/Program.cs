using Hermes.Configuration;
using Hermes.Core;
using Hermes.Core.Deferment;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

namespace Clientele.Messaging.DeferrmentService
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

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
            Configure.Endpoint(Settings.DefermentEndpoint.ToString(), new AutofacAdapter())
                     .UseConsoleWindowLogger()
                     .UseJsonSerialization()
                     .UseDefermentBus()
                     .UseSqlTransport(ConnectionString)
                     .UseSqlStorage(ConnectionString)
                     .Start();

            var timeoutProcessor = Settings.RootContainer.GetInstance<ITimeoutProcessor>();
            timeoutProcessor.Start();
        }
    }
}
