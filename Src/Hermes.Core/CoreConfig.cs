using Hermes.Configuration;
using Hermes.Core.Deferment;
using Hermes.Ioc;

namespace Hermes.Core
{
    public static class CoreConfig
    {
        public static IConfigureBus UsingUnicastBus(this IConfigureBus config)
        {
            Settings.Builder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Processor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Bus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Publisher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            
            return config;
        }

        public static IConfigureBus UsingDefermentBus(this IConfigureBus config) 
        {
            Settings.Builder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<DefermentProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Bus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Publisher>(DependencyLifecycle.SingleInstance);

            return config;
        }
    }
}
