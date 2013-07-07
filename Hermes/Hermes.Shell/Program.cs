using System;

using Autofac;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.Messages;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization;
using Hermes.Serialization.Json;
using Hermes.Transports;
using Hermes.Transports.SqlServer;

namespace Hermes.Shell
{
    class Program
    {
        private const string connection = @"Data Source=CHANDRA\SQLEXPRESS;Initial Catalog=AsbaBank;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";
        static Address testEndpoint;
        static IMessageBus bus;

        static void Main(string[] args)
        {
            Initialize();

            while (true)
            {
                var message1 = new RegisterNewClient
                {
                    Hello = DateTime.Now.ToShortTimeString()
                };

                var message2 = new TransferMoneyToAccount
                {
                    Hello = DateTime.Now.ToShortTimeString()
                };

                bus.Send(message1, message2);
                System.Threading.Thread.Sleep(100);       
            }
        }

        private static void Initialize()
        {
            Configure.With()
                     .MessageQueueConnectionString(connection)
                     .ObjectBuilder(ConfigureBuilder())
                     .ConsoleWindowLogger()
                     .NumberOfWorkers(4);

            InitializeQueues();
            InitializeRoutes();
            StartMessageBus();
        }

        private static void StartMessageBus()
        {
            bus = Settings.Builder.GetInstance<IMessageBus>();
            var busStarter = Settings.Builder.GetInstance<IStartableMessageBus>();
            busStarter.Start(testEndpoint);
        }

        private static void InitializeQueues()
        {
            testEndpoint = Address.Parse("Queues.Testing.MyTestQueue");
            var queueCreator = new SqlServerQueueCreator();
            queueCreator.CreateQueueIfNecessary(testEndpoint);
        }

        private static void InitializeRoutes()
        {
            var routeConfig = Settings.Builder.GetInstance<IRegisterMessageRoute>();

            routeConfig
                .RegisterRoute(typeof (RegisterNewClient), testEndpoint)
                .RegisterRoute(typeof (TransferMoneyToAccount), testEndpoint);
        }

        private static AutofacServiceAdapter ConfigureBuilder()
        {
            var builder = new ContainerBuilder();
            var autofacServiceAdapter = new AutofacServiceAdapter();
            builder.RegisterType<JsonObjectSerializer>().As<ISerializeObjects>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<SqlMessageDequeueStrategy>().As<IMessageDequeueStrategy>().SingleInstance();
            builder.RegisterType<SqlServerMessageSender>().As<ISendMessages>().SingleInstance();
            builder.RegisterType<MessageProcessor>().As<IProcessMessages>().SingleInstance();
            builder.RegisterType<MessageTransport>().As<ITransportMessages>().SingleInstance();
            builder.RegisterType<SqlServerMessageReceiver>().As<IDequeueMessages>().SingleInstance();
            builder.RegisterType<MessageBus>().As<IMessageBus>().As<IStartableMessageBus>().SingleInstance();
            
            builder.RegisterType<MessageRouter>()
                   .As<IRouteMessageToEndpoint>()
                   .As<IRegisterMessageRoute>()
                   .SingleInstance();

            builder.RegisterType<MessageHandlerFactory>().As<IBuildMessageHandlers>().InstancePerLifetimeScope();
            builder.RegisterType<MessageDispatcher>().As<IDispatchMessagesToHandlers>().InstancePerLifetimeScope();
           
            builder.RegisterType<MessageHandler>()
                   .As<IHandleMessage<RegisterNewClient>>()
                   .As<IHandleMessage<TransferMoneyToAccount>>()
                   .InstancePerLifetimeScope();

            builder.RegisterInstance(autofacServiceAdapter).As<IObjectBuilder>();

            autofacServiceAdapter.lifetimeScope = builder.Build();
            return autofacServiceAdapter;
        }
    }
}
