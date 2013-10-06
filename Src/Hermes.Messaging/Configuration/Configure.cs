using System;
using System.Reflection;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Routing;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Configuration
{
    public class Configure : IConfigureEndpoint
    {
        private static readonly Configure Instance;

        static Configure()
        {
            Instance = new Configure();
        }

        private Configure()
        {
        }


        IConfigureEndpoint IConfigureEndpoint.UseConsoleWindowLogger()
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            return this;
        }

        IConfigureEndpoint IConfigureEndpoint.Logger(Func<Type, ILog> buildLogger)
        {
            LogFactory.BuildLogger = buildLogger;
            return this;
        }

        public static IConfigureEndpoint ClientEndpoint(string endpointName, IContainerBuilder containerBuilder)
        {
            Mandate.ParameterNotNullOrEmpty(endpointName, "endpointName");
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");

            containerBuilder.RegisterSingleton<IContainerBuilder>(containerBuilder);
            Settings.Builder = containerBuilder;
            Settings.SetEndpointName(endpointName);
            Settings.IsClientEndpoint = true;
            return Instance;
        }


        public static IConfigureEndpoint ServerEndpoint(string endpointName, IContainerBuilder containerBuilder)
        {
            Mandate.ParameterNotNullOrEmpty(endpointName, "endpointName");
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");

            containerBuilder.RegisterSingleton<IContainerBuilder>(containerBuilder);
            Settings.Builder = containerBuilder;
            Settings.SetEndpointName(endpointName);
            return Instance;
        }

        IConfigureEndpoint IConfigureEndpoint.NumberOfWorkers(int numberOfWorkers)
        {
            Settings.NumberOfWorkers = numberOfWorkers;
            return this;
        }

        IConfigureEndpoint IConfigureEndpoint.ScanForHandlersIn(params Assembly[] assemblies)
        {
            Settings.Builder.RegisterMessageHandlers(assemblies);
            return this;
        }

        IConfigureEndpoint IConfigureEndpoint.RegisterMessageRoute<TMessage>(Address endpointAddress)
        {
            var router = Settings.RootContainer.GetInstance<IRegisterMessageRoute>();
            router.RegisterRoute(typeof(TMessage), endpointAddress);
            return this;
        }

        public IConfigureEndpoint SubscribeToEvent<TMessage>()
        {
            Settings.Subscriptions.Subscribe<TMessage>();
            return this;
        }

        public IConfigureEndpoint UseDistributedTransaction()
        {
            Settings.UseDistributedTransaction = true;
            return this;
        }

        IConfigureEndpoint IConfigureEndpoint.FirstLevelRetryPolicy(int attempts, TimeSpan delay)
        {
            Settings.FirstLevelRetryAttempts = attempts;
            Settings.FirstLevelRetryDelay = delay;
            return this;
        }

        IConfigureEndpoint IConfigureEndpoint.SecondLevelRetryPolicy(int attempts, TimeSpan delay)
        {
            Settings.SecondLevelRetryAttempts = attempts;
            Settings.SecondLevelRetryDelay = delay;
            return this;
        }

        void IConfigureEndpoint.Start()
        {
            var queueCreator = Settings.RootContainer.GetInstance<ICreateQueues>();
            queueCreator.CreateQueueIfNecessary(Address.Local);

            if (Settings.IsClientEndpoint)
            {
                queueCreator.Purge(Address.Local);
            }
            else
            {
                queueCreator.CreateQueueIfNecessary(Settings.ErrorEndpoint);
                queueCreator.CreateQueueIfNecessary(Settings.DefermentEndpoint);
            }

            var busStarter = Settings.RootContainer.GetInstance<IStartableMessageBus>();
            busStarter.Start();
        }

        void IConfigureEndpoint.Stop()
        {
            var busStarter = Settings.RootContainer.GetInstance<IStartableMessageBus>();
            busStarter.Stop();
        }
    }
}