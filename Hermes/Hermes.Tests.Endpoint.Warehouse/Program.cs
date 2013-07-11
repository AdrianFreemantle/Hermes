using Autofac;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.Messages;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization;
using Hermes.Serialization.Json;
using Hermes.Subscriptions;
using Hermes.Tests.Messages;
using Hermes.Transports;
using Hermes.Transports.SqlServer;

namespace Hermes.Tests.Endpoint.Warehouse
{
    class Program
    {
        private const string connection = @"Data Source=CHANDRA\SQLEXPRESS;Initial Catalog=AsbaBank;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";
        static Address thisEndpoint;
        static IMessageBus bus;

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
            Configure.With()
                     .MessageQueueConnectionString(connection)
                     .ObjectBuilder(ConfigureBuilder())
                     .ConsoleWindowLogger()
                     .NumberOfWorkers(1);

            InitializeQueues();
            InitializeRoutes();
            StartMessageBus();
            SubscribeToEvents();
        }

        private static void StartMessageBus()
        {
            bus = Settings.Builder.GetInstance<IMessageBus>();
            var busStarter = Settings.Builder.GetInstance<IStartableMessageBus>();
            busStarter.Start(thisEndpoint);
        }

        private static void SubscribeToEvents()
        {
            bus.Subscribe<ShoesSold>();
        }

        private static void InitializeQueues()
        {
            thisEndpoint = Address.Parse("Queues.Warehouse");
            var queueCreator = new SqlQueueCreator();
            queueCreator.CreateQueueIfNecessary(thisEndpoint);
        }

        private static void InitializeRoutes()
        {
            var routeConfig = Settings.Builder.GetInstance<IRegisterMessageRoute>();

            routeConfig
                .RegisterRoute(typeof(ShoesSold), thisEndpoint)
                .RegisterRoute(typeof(OrderShipped), Address.Parse("Queues.Shell"));
        }

        private static AutofacServiceAdapter ConfigureBuilder()
        {
            var builder = new ContainerBuilder();
            var autofacServiceAdapter = new AutofacServiceAdapter();
            builder.RegisterType<JsonObjectSerializer>().As<ISerializeObjects>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<SqlMessageDequeueStrategy>().As<IMessageDequeueStrategy>().SingleInstance();
            builder.RegisterType<SqlMessageSender>().As<ISendMessages>().SingleInstance();
            builder.RegisterType<MessageProcessor>().As<IProcessMessages>().SingleInstance();
            builder.RegisterType<MessageTransport>().As<ITransportMessages>().SingleInstance();
            builder.RegisterType<SqlMessageReceiver>().As<IDequeueMessages>().SingleInstance();
            builder.RegisterType<MessageBus>().As<IMessageBus>().As<IStartableMessageBus>().SingleInstance();
            builder.RegisterType<SqlSubscriptionStorage>().As<IStoreSubscriptions>().SingleInstance();
            builder.RegisterType<SqlMessagePublisher>().As<IPublishMessages>().SingleInstance();

            builder.RegisterType<MessageRouter>()
                   .As<IRouteMessageToEndpoint>()
                   .As<IRegisterMessageRoute>()
                   .SingleInstance();

            builder.RegisterType<MessageHandlerFactory>().As<IBuildMessageHandlers>().InstancePerLifetimeScope();
            builder.RegisterType<MessageDispatcher>().As<IDispatchMessagesToHandlers>().InstancePerLifetimeScope();

            builder.RegisterType<WarehouseHandler>()
                   .As<IHandleMessage<ShoesSold>>()
                   .InstancePerLifetimeScope();

            builder.RegisterInstance(autofacServiceAdapter).As<IObjectBuilder>();

            autofacServiceAdapter.lifetimeScope = builder.Build();
            return autofacServiceAdapter;
        }
    }
}
