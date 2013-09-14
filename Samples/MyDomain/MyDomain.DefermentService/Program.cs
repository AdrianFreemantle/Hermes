using Hermes.Configuration;
using Hermes.Core;
using Hermes.Core.Deferment;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

namespace MyDomain.DefermentService
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MyDomain;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

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
            Configure.Environment(new AutofacAdapter())
                     .ConsoleWindowLogger();

            Configure.Bus(Settings.DefermentEndpoint)
                     .UsingJsonSerialization()
                     .UsingDefermentBus()
                     .UsingSqlTransport(ConnectionString)
                     .UsingSqlStorage(ConnectionString)
                     .Start();

            var timeoutProcessor = Settings.RootContainer.GetInstance<ITimeoutProcessor>();
            timeoutProcessor.Start();
        }
    }


}
