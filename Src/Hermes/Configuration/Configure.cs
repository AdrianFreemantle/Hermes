using System;
using System.Reflection;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Transports;

namespace Hermes.Configuration
{
    public class Configure : IConfigureEnvironment, IConfigureBus
    {
        private static readonly Configure instance;

        static Configure()
        {
            instance = new Configure();
        }

        private Configure()
        {
            
        }

        public static IConfigureEnvironment Environment(IObjectBuilder objectBuilder)
        {
            objectBuilder.RegisterSingleton<IObjectBuilder>(objectBuilder);
            Settings.Builder = objectBuilder;
            return instance;
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
            return instance;
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
            var router = Settings.Builder.GetInstance<IRegisterMessageRoute>();
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
            var queueCreator = Settings.Builder.GetInstance<ICreateQueues>();
            queueCreator.CreateQueueIfNecessary(Settings.ThisEndpoint);

            var busStarter = Settings.Builder.GetInstance<IStartableMessageBus>();
            busStarter.Start();
        }

        void IConfigureBus.Stop()
        {
            var busStarter = Settings.Builder.GetInstance<IStartableMessageBus>();
            busStarter.Stop();
        }
    }
}