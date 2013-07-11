﻿using System;

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
                System.Threading.Thread.Sleep(100);       
                Console.WriteLine("Selling Shoes");
                bus.Send(new SellShoes {ShoeTypeId = 1, Size = 2});
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
            SubscribeToEvents();
        }

        private static void StartMessageBus()
        {
            bus = Settings.Builder.GetInstance<IMessageBus>();
            var busStarter = Settings.Builder.GetInstance<IStartableMessageBus>();
            busStarter.Start(testEndpoint);
        }

        private static void SubscribeToEvents()
        {
            bus.Subscribe<OrderShipped>();
        }

        private static void InitializeQueues()
        {
            testEndpoint = Address.Parse("Queues.Shell");
            var queueCreator = new SqlQueueCreator();
            queueCreator.CreateQueueIfNecessary(testEndpoint);
        }

        private static void InitializeRoutes()
        {
            var routeConfig = Settings.Builder.GetInstance<IRegisterMessageRoute>();

            routeConfig
                .RegisterRoute(typeof(OrderShipped), testEndpoint)
                .RegisterRoute(typeof (SellShoes), Address.Parse("Queues.Sales"));
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

            builder.RegisterType<ShellHandler>()
                   .As<IHandleMessage<OrderShipped>>()
                   .InstancePerLifetimeScope();

            builder.RegisterInstance(autofacServiceAdapter).As<IObjectBuilder>();

            autofacServiceAdapter.lifetimeScope = builder.Build();
            return autofacServiceAdapter;
        }
       
    }
}
