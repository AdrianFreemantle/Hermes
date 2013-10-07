using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using Hermes;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using RequestResponseMessages;

namespace Requestor
{
    class Program
    {
        private static readonly Random Rand = new Random();
        private static ILog Logger;
        private const string ConnectionString = @"Data Source=CG-T-SQL-03V;Initial Catalog=CG_T_DB_MSGBRKR;User ID=CG_T_USR_SYNAFreemantle;Password=vimes Sep01";

        private static void Main(string[] args)
        {
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            Configure
                .ClientEndpoint("Requestor", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .RegisterMessageRoute<AddNumbers>(Address.Parse("Respondor"))
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .Start();

            Logger = LogFactory.BuildLogger(typeof(Program));        
            var token = new CancellationTokenSource(TimeSpan.FromHours(2));

            Console.WriteLine("Press any key to send a request");
            Console.ReadKey();

            while (!token.IsCancellationRequested)
            {
                Settings.MessageBus.Send(Guid.NewGuid(), NewCalculation()).Register(Completed);
                Console.ReadKey();
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        public static AddNumbers NewCalculation()
        {
            var x = Rand.Next(0, 10);
            var y = Rand.Next(0, 10);
            Logger.Info("Adding numbers {0} and {1}", x, y);
            return new AddNumbers { X = x, Y = y };
        }

        public static void Completed(CompletionResult cr)
        {
            if (cr.ErrorCode == 0)
            {
                var additionResult = (AdditionResult)cr.Messages.FirstOrDefault();
                Logger.Info("Result is {0}", additionResult.Result);
            }
            else
            {
                Logger.Error("Error result returned");
            }
        }
    }
    
    public class Handler : IHandleMessage<AdditionResult>
    {
        public void Handle(AdditionResult message)
        {
            Console.WriteLine("Result is {0}", message.Result);
        }
    }
}
