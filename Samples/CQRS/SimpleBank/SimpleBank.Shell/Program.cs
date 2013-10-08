using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Hermes;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using SimpleBank.Messages;
using SimpleBank.Messages.Commands;

namespace SimpleBank.Shell
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";
        static ILog Logger;

        private static void Main(string[] args)
        {
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            Configure
                .ClientEndpoint("SimpleBank.Atm", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .RegisterMessageRoute<ClosePortfolio>(Address.Parse("SimpleBank.ApplicationService"))
                .RegisterMessageRoute<CreditAccount>(Address.Parse("SimpleBank.ApplicationService"))
                .RegisterMessageRoute<OpenAccount>(Address.Parse("SimpleBank.ApplicationService"))
                .RegisterMessageRoute<DebitAccount>(Address.Parse("SimpleBank.ApplicationService"))
                .RegisterMessageRoute<OpenPortfolio>(Address.Parse("SimpleBank.ApplicationService"))
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .Start();

            Logger = LogFactory.BuildLogger(typeof (Program));

            while (true)
            {
                Console.ReadKey();

                var id = PortfolioId.GenerateId();

                Console.WriteLine("Sending OpenPortfolio command");
                Settings.MessageBus.Send(new OpenPortfolio
                {
                    PortfolioId = id,
                    AccountType = AccountType.Savings,
                    InitialDeposit = 500
                }).Register(HandleCompletion).Wait();

                Console.WriteLine("Sending OpenPortfolio command");
                Settings.MessageBus.Send(new CreditAccount
                {
                    PortfolioId = id,
                    AccountType = AccountType.Savings,
                    Amount = 50
                }).Register(HandleCompletion).Wait();

                Console.WriteLine("Sending ClosePortfolio command");
                Settings.MessageBus.Send(new ClosePortfolio
                {
                    PortfolioId = id,
                }).Register(HandleCompletion).Wait();

                Console.WriteLine("Sending CreditAccount command");
                Settings.MessageBus.Send(new CreditAccount
                {
                    PortfolioId = id,
                    AccountType = AccountType.Savings,
                    Amount = 50
                }).Register(HandleCompletion).Wait();

                Console.WriteLine("Sending DebitAccount command");
                Settings.MessageBus.Send(new DebitAccount
                {
                    PortfolioId = id,
                    AccountType = AccountType.Savings,
                    Amount = 550
                }).Register(HandleCompletion).Wait();

                Console.WriteLine("Sending DebitAccount command");
                Settings.MessageBus.Send(new DebitAccount
                {
                    PortfolioId = id,
                    AccountType = AccountType.Savings,
                    Amount = 10
                }).Register(HandleCompletion).Wait();
            }
        }

        public static void HandleCompletion(CompletionResult result)
        {
            if (result.ErrorCode != 0)
            {
                if (result.Messages.Any())
                {
                    Logger.Error("Request Failed : {0}", result.Messages[0]);
                }
                else
                {
                    Logger.Error("Request Failed");
                }
            }
            else
            {
                Logger.Info("Request Completed");
            }
        }
    }
}
