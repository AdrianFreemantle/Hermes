using System;
using System.Reflection;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Transports;

namespace Hermes.Configuration
{
    public class Configure : IConfigureEnvironment, IConfigureBus
    {
        private static readonly Configure Instance;

        static Configure()
        {
            Instance = new Configure();
        }

        private Configure()
        {
        }

        public static IConfigureEnvironment Environment(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterSingleton<IContainerBuilder>(containerBuilder);
            Settings.Builder = containerBuilder;
            return Instance;
        }

        IConfigureEnvironment IConfigureEnvironment.ConsoleWindowLogger()
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            return this;
        }

        IConfigureEnvironment IConfigureEnvironment.Logger(Func<Type, ILog> buildLogger)
        {
            LogFactory.BuildLogger = buildLogger;
            return this;
        }

        public static IConfigureBus Bus(Address thisEndpoint)
        {
            if (Settings.Builder == null)
            {
                throw new EnvironmentConfigurationException("You must first configure the environment settings before attempting to configure the Bus");
            }

            Settings.ThisEndpoint = thisEndpoint;
            return Instance;
        }

        IConfigureBus IConfigureBus.NumberOfWorkers(int numberOfWorkers)
        {
            Settings.NumberOfWorkers = numberOfWorkers;
            return this;
        }

        IConfigureBus IConfigureBus.ScanForHandlersIn(params Assembly[] assemblies)
        {
            Settings.Builder.RegisterHandlers(assemblies);
            return this;
        }

        IConfigureBus IConfigureBus.RegisterMessageRoute<TMessage>(Address endpointAddress)
        {
            var router = Settings.RootContainer.GetInstance<IRegisterMessageRoute>();
            router.RegisterRoute(typeof(TMessage), endpointAddress);
            return this;
        }

        public IConfigureBus SubscribeToEvent<TMessage>()
        {
            Settings.Subscriptions.Subscribe<TMessage>();
            return this;
        }

        void IConfigureBus.Start()
        {
            var queueCreator = Settings.RootContainer.GetInstance<ICreateQueues>();
            queueCreator.CreateQueueIfNecessary(Settings.ThisEndpoint);

            var busStarter = Settings.RootContainer.GetInstance<IStartableMessageBus>();
            busStarter.Start();
        }

        void IConfigureBus.Stop()
        {
            var busStarter = Settings.RootContainer.GetInstance<IStartableMessageBus>();
            busStarter.Stop();
        }
    }
}