﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Attributes;
using Hermes.Failover;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Hermes.Reflection;

// ReSharper disable CheckNamespace
// ReSharper disable RedundantExtendsListEntry
namespace Hermes
{
    public class Configure : IConfigureEndpoint, IConfigureWorker
    {
        private static readonly ILog Logger = LogFactory.Build<Configure>();
        private static readonly Configure Instance;
        private static IContainerBuilder containerBuilder;        

        static Configure()
        {
            Instance = new Configure();
        }    

        private Configure()
        {
            CriticalError.OnCriticalError += OnCriticalError;
        }

        public static Configure Initialize(string endpointName, IContainerBuilder builder)
        {
            Mandate.ParameterNotNullOrEmpty(endpointName, "endpointName");
            Mandate.ParameterNotNull(builder, "builder");

            containerBuilder = builder;
            containerBuilder.RegisterSingleton(containerBuilder);

            var busRegistrar = new MessageBusDependencyRegistrar();
            busRegistrar.Register(containerBuilder);           

            Settings.SetEndpointName(endpointName);
            Settings.RootContainer = containerBuilder.BuildContainer();            

            return Instance;
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

        IConfigureEndpoint IConfigureEndpoint.DisablePerformanceMonitoring()
        {
            Logger.Debug("Diabling performance monitoring");
            Settings.DisablePerformanceMonitoring = true;
            return this;
        }

        public IConfigureEndpoint DisableMessageAudit()
        {
            Logger.Debug("Disabling message audit");
            Settings.DisableMessageAudit = true;
            return this;
        }

        public IConfigureEndpoint NumberOfWorkers(int numberOfWorkers)
        {
            Logger.Debug("Setting number of working threads to {0}", numberOfWorkers);
            Settings.NumberOfWorkers = numberOfWorkers;
            return this;
        }

        public IConfigureEndpoint RegisterMessageRoute<TMessage>(Address endpointAddress)
        {
            var router = Settings.RootContainer.GetInstance<IRegisterMessageRoute>();
            router.RegisterRoute(typeof(TMessage), endpointAddress);
            return this;
        }

        public IConfigureEndpoint DisableHeartbeatService()
        {
            Logger.Debug("Disabling Heartbeat service");
            Settings.DisableHeartbeatService = true;
            return this;
        }

        public IConfigureEndpoint DisableDistributedTransactions()
        {
            Logger.Debug("Disabling distributed transactions");
            Settings.DisableDistributedTransactions = true;
            return this;
        }

        IConfigureWorker IConfigureWorker.SecondLevelRetryPolicy(int attempts, TimeSpan delay)
        {
            Settings.SecondLevelRetryAttempts = attempts;
            Settings.SecondLevelRetryDelay = delay;
            return this;
        }

        IConfigureWorker IConfigureWorker.FirstLevelRetryPolicy(int attempts)
        {
            Settings.FirstLevelRetryAttempts = attempts;
            return this;
        }

        IConfigureWorker IConfigureWorker.FlushQueueOnStartup(bool flush)
        {
            Settings.FlushQueueOnStartup = flush;
            return this;
        }

        public IConfigureWorker CircuitBreakerPolicy(int circuitBreakerThreshold, TimeSpan circuitBreakerReset)
        {
            Settings.CircuitBreakerThreshold = circuitBreakerThreshold;
            Settings.CircuitBreakerReset = circuitBreakerReset;
            return this;
        }

        public IConfigureWorker DisablePerformanceMonitoring()
        {
            Logger.Debug("Disabling performance monitoring");
            Settings.DisablePerformanceMonitoring = true;
            return this;
        }

        public IConfigureEndpoint RegisterDependencies<T>() where T : IRegisterDependencies, new()
        {
            return RegisterDependencies(new T());
        }

        public IConfigureEndpoint RegisterDependencies(IRegisterDependencies registerationHolder)
        {
            registerationHolder.Register(containerBuilder);
            return this;
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

        public IConfigureEndpoint SendOnlyEndpoint()
        {
            Logger.Debug("Setting endpoint and send only");
            Settings.IsSendOnly = true;
            return this;
        }

        public IConfigureEndpoint UserNameResolver(Func<string> userNameResolver)
        {
            Settings.UserNameResolver = userNameResolver;
            return this;
        }

        public IConfigureEndpoint EndpointName(string name)
        {
            Logger.Debug("Setting endpoint name to {0}", name);
            Settings.SetEndpointName(name);
            return this;
        }

        public IConfigureEndpoint EnableCommandValidators()
        {
            Logger.Debug("Enabling command validators");
            Settings.EnableCommandValidationClasses = true;
            return this;
        }

        internal void Start()
        {
            Logger.Debug("Beginning startup process");
            ComponentScanner.Scan(containerBuilder);

            MapMessageTypes();
            RunInitializers();
            SubscribeToEvents();
            CreateQueues();
            StartServices();

            Logger.Debug("Startup process completed");
        }

        private static void MapMessageTypes()
        {
            Logger.Debug("Mapping message types");
            var mapper = Settings.RootContainer.GetInstance<ITypeMapper>();
            mapper.Initialize(HandlerCache.GetAllHandledMessageContracts());
        }

        private static void StartServices()
        {
            Logger.Debug("Starting services...");
            var startableObjects = Settings.RootContainer.GetAllInstances<IAmStartable>();

            foreach (var startableObject in startableObjects)
            {
                Logger.Debug("Starting {0}", startableObject.GetType().FullName);
                startableObject.Start();
            }
        }

        private static void RunInitializers()
        {
            Logger.Debug("Running initializers.");
            var intializers = Settings.RootContainer.GetAllInstances<INeedToInitializeSomething>().ToArray();

            var orderedInitalizers = intializers
                .Where(something => something.HasAttribute<InitializationOrderAttribute>())
                .OrderBy(i => i.GetCustomAttributes<InitializationOrderAttribute>().First().Order);

            var unorderedInitalizers = intializers
                .Where(something => !something.HasAttribute<InitializationOrderAttribute>());

            RunIntializers(orderedInitalizers);
            RunIntializers(unorderedInitalizers);
        }

        private static void RunIntializers(IEnumerable<INeedToInitializeSomething> intializers)
        {
            foreach (var init in intializers)
            {
                Logger.Debug("Running initializer {0}", init.GetType().FullName);
                init.Initialize();
            }
        }

        private static void CreateQueues()
        {
            Logger.Debug("Creating message queues.");
            var queueCreator = Settings.RootContainer.GetInstance<ICreateMessageQueues>();
            queueCreator.CreateQueueIfNecessary(Settings.MonitoringEndpoint);


            if(Settings.IsSendOnly)
                return;

            queueCreator.CreateQueueIfNecessary(Address.Local);
            queueCreator.CreateQueueIfNecessary(Settings.ErrorEndpoint);
            queueCreator.CreateQueueIfNecessary(Settings.AuditEndpoint);

            if (Settings.FlushQueueOnStartup)
            {
                queueCreator.Purge(Address.Local);
            }
        }

        private static void SubscribeToEvents()
        {
            if (!Settings.AutoSubscribeEvents)
            {
                 return;   
            }

            Logger.Debug("Subscribing to events");

            foreach (var eventType in HandlerCache.GetAllHandledMessageContracts().Where(type => Settings.IsEventType(type)))
            {
                if (typeof (IDomainEvent).IsAssignableFrom(eventType) && !Settings.SubsribeToDomainEvents)
                        continue;

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

        private void OnCriticalError(CriticalErrorEventArgs e)
        {
            Stop();
        }
    }
}
// ReSharper restore RedundantExtendsListEntry
// ReSharper restore CheckNamespace
