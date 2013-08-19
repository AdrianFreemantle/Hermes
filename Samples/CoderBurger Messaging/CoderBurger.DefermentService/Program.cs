using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Core.Deferment;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Transports.SqlServer;

namespace CoderBurger.DefermentService
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
            Configure.Environment(new AutofacAdapter())
                     .ConsoleWindowLogger();

            Configure.Bus(Settings.DefermentEndpoint)
                     .UsingJsonSerialization()
                     .UsingDefermentBus(new InMemoryTimeoutPersistence())
                     .UsingSqlTransport(ConnectionString)
                     .Start();

            var timeoutProcessor = Settings.RootContainer.GetInstance<ITimeoutProcessor>();
            timeoutProcessor.Start();
        }
    }
}
