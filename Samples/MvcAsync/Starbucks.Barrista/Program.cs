using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using Starbucks.Messages;

namespace Starbucks.Barrista
{
    class Program
    {
        private const string ConnectionString = @"Data Source=CG-T-SQL-03V;Initial Catalog=CG_T_DB_MSGBRKR;User ID=CG_T_USR_SYNAFreemantle;Password=vimes Sep01";

        static void Main(string[] args)
        {
            ConfigureHermes();

            while (true)
            {
                System.Threading.Thread.Sleep(100);    
            }
        }

        private static void ConfigureHermes()
        {
            Configure
                .ServerEndpoint("Barrista", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .NumberOfWorkers(5)
                .Start();
        }
    }

    public class Barrista : IHandleMessage<BuyCoffee>
    {
        private readonly IMessageBus bus;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (Barrista));

        public Barrista(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(BuyCoffee message)
        {
            Logger.Info("Barista is attempting to prepare your order");
            System.Threading.Thread.Sleep(2000);
            
            if (DateTime.Now.Ticks % 2 == 0)
            {
                Logger.Info("Barista is completed your order");
                bus.Return(ErrorCodes.Success);
            }
            else
            {
                Logger.Info("Out of coffee!");
                bus.Return(ErrorCodes.Error);
            }
        }
    }
}
