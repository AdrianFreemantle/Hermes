using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Routing;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Configuration
{
    public class Configure : IConfigureEndpoint, IConfigureWorker
    {
        private static readonly Configure Instance;
        private static IContainerBuilder containerBuilder;        

        static Configure()
        {
            Instance = new Configure();
        }

        private Configure()
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Verbose;
        }

        internal static Configure ClientEndpoint(string endpointName, IContainerBuilder builder)
        {
            Settings.IsClientEndpoint = true;
            ConfigureEndpoint(endpointName, builder);
            return Instance;
        }

        internal static Configure WorkerEndpoint(string endpointName, IContainerBuilder builder)
        {
            ConfigureEndpoint(endpointName, builder);            
            return Instance;
        }

        private static void ConfigureEndpoint(string endpointName, IContainerBuilder builder)
        {
            Mandate.ParameterNotNullOrEmpty(endpointName, "endpointName");
            Mandate.ParameterNotNull(builder, "builder");

            containerBuilder = builder;
            containerBuilder.RegisterSingleton(containerBuilder);
            UnicastBusDependancyRegistrar.Register(containerBuilder);

            using (var scanner = new AssemblyScanner())
            {
                containerBuilder.RegisterMessageHandlers(scanner.Assemblies);
            }

            Settings.SetEndpointName(endpointName);
            Settings.RootContainer = containerBuilder.BuildContainer();
        }

        public IConfigureEndpoint DefineMessageAs(Func<Type, bool> isMessageRule)
        {
            Settings.IsMessageType = isMessageRule;
            return this;
        }

        public IConfigureEndpoint DefineCommandAs(Func<Type, bool> isCommandRule)
        {
            Settings.IsCommandType = isCommandRule;
            return this;
        }

        public IConfigureEndpoint DefineEventAs(Func<Type, bool> isEventRule)
        {
            Settings.IsEventType = isEventRule;
            return this;
        }

        internal static Func<Type, bool> IsCommandType { get; set; }
        internal static Func<Type, bool> IsEventType { get; set; }

        public IConfigureEndpoint NumberOfWorkers(int numberOfWorkers)
        {
            Settings.NumberOfWorkers = numberOfWorkers;
            return this;
        }

        public IConfigureEndpoint RegisterMessageRoute<TMessage>(Address endpointAddress)
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

        IConfigureWorker IConfigureWorker.UseDistributedTransaction()
        {
            Settings.UseDistributedTransaction = true;
            return this;
        }

        IConfigureWorker IConfigureWorker.SecondLevelRetryPolicy(int attempts, TimeSpan delay)
        {
            Settings.SecondLevelRetryAttempts = attempts;
            Settings.SecondLevelRetryDelay = delay;
            return this;
        }

        public IConfigureEndpoint FirstLevelRetryPolicy(int attempts, TimeSpan delay)
        {
            Settings.FirstLevelRetryAttempts = attempts;
            Settings.FirstLevelRetryDelay = delay;
            return this;
        }

        public IConfigureEndpoint RegisterDependancies(IRegisterDependancies registerationHolder)
        {
            registerationHolder.Register(containerBuilder);
            return this;
        }

        internal void Start()
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

            var busStarter = Settings.RootContainer.GetInstance<IAmStartable>();
            busStarter.Start();
        }

        internal void Stop()
        {
            var busStarter = Settings.RootContainer.GetInstance<IAmStartable>();
            busStarter.Stop();
        }

        public IConfigureEndpoint DefiningMessagesAs(Func<Type, bool> definesMessageType)
        {
            Settings.IsMessageType = definesMessageType;
            return this;
        }

        public IConfigureEndpoint DefiningCommandsAs(Func<Type, bool> definesCommandType)
        {
            Settings.IsCommandType = definesCommandType;
            return this;
        }

        public IConfigureEndpoint DefiningEventsAs(Func<Type, bool> definesEventType)
        {
            Settings.IsEventType = definesEventType;
            return this;
        }      
    }
}