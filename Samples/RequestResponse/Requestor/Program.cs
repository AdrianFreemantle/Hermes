﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using Hermes;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using RequestResponseMessages;

namespace Requestor
{
    /*
     * NB !!! Remember to manually flush the queues before starting or the expected results wont match
     */

    class Program
    {
        private static Random rand = new Random();
        private static ILog Logger;
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            Configure
                .Endpoint("Requestor", new AutofacAdapter())
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
            var token = new CancellationTokenSource(TimeSpan.FromHours(1));

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
            var x = rand.Next(0, 10);
            var y = rand.Next(0, 10);
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
                Logger.Fatal("No result returned");
            }

            Console.WriteLine("Press any key to send a request");
        }
    }    
}
