using Hermes.Configuration;
using Hermes.Core.Deferment;
using Hermes.Ioc;

namespace Hermes.Core
{
    public static class CoreConfig
    {
        public static IConfigureBus UsingUnicastBus(this IConfigureBus config)
        {
            Settings.Builder.RegisterType<MessageHandlerFactory>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageDispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageRouter>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<InMemoryBus>(DependencyLifecycle.InstancePerLifetimeScope);
            
            return config;
        }

        public static IConfigureBus UsingDefermentBus(this IConfigureBus config, IPersistTimeouts timeoutStore) 
        {
            Settings.Builder.RegisterType<MessageHandlerFactory>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageDispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<DefermentProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageRouter>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterSingleton<IPersistTimeouts>(timeoutStore);

            return config;
        }
    }
}
