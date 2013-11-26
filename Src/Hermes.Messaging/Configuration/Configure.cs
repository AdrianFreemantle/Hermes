using System;
using System.Linq;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration.MessageHandlerCache;

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

            var busRegistrar = new MessageBusDependencyRegistrar();
            busRegistrar.Register(containerBuilder);           

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

        public IConfigureEndpoint DontUseDistributedTransaction()
        {
            Settings.UseDistributedTransaction = false;
            return this;
        }

        IConfigureWorker IConfigureWorker.SecondLevelRetryPolicy(int attempts, TimeSpan delay)
        {
            Settings.SecondLevelRetryAttempts = attempts;
            Settings.SecondLevelRetryDelay = delay;
            return this;
        }

        IConfigureWorker IConfigureWorker.FirstLevelRetryPolicy(int attempts, TimeSpan delay)
        {
            Settings.FirstLevelRetryAttempts = attempts;
            Settings.FirstLevelRetryDelay = delay;
            return this;
        }

        public IConfigureEndpoint RegisterDependencies(IRegisterDependencies registerationHolder)
        {
            registerationHolder.Register(containerBuilder);
            return this;
        }

        internal void Start()
        {
            ComponentScanner.Scan(containerBuilder);

            RunInitializers();
            SubscribeToEvents();
            CreateQueues();
            StartServices();

            var modules = Settings.RootContainer.GetAllInstances<IModule>().ToList();
        }

        private static void StartServices()
        {
            var startableObjects = Settings.RootContainer.GetAllInstances<IAmStartable>();

            foreach (var startableObject in startableObjects)
            {
                startableObject.Start();
            }
        }

        private static void RunInitializers()
        {
            var intializers = Settings.RootContainer.GetAllInstances<INeedToInitializeSomething>();

            foreach (var init in intializers)
            {
                init.Initialize();
            }
        }

        private static void CreateQueues()
        {
            var queueCreator = Settings.RootContainer.GetInstance<ICreateQueues>();
            queueCreator.CreateQueueIfNecessary(Address.Local);
            queueCreator.CreateQueueIfNecessary(Settings.ErrorEndpoint);
            queueCreator.CreateQueueIfNecessary(Settings.AuditEndpoint);

            if (Settings.IsClientEndpoint)
            {
                queueCreator.Purge(Address.Local);
            }
        }

        private static void SubscribeToEvents()
        {            
            foreach (var eventType in HandlerCache.GetAllHandledMessageContracts().Where(type => Settings.IsEventType(type)))
            {
                Settings.Subscriptions.Subscribe(eventType);
            }
        }

        internal void Stop()
        {
            var startableObjects = Settings.RootContainer.GetAllInstances<IAmStartable>();

            foreach (var startableObject in startableObjects)
            {
                startableObject.Stop();
            }
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