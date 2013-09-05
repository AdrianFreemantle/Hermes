using Hermes.Configuration;
using Hermes.Core.Deferment;
using Hermes.Ioc;

namespace Hermes.Core
{
    public static class CoreConfig
    {
        public static IConfigureEndpoint UseUnicastBus(this IConfigureEndpoint config)
        {
            Settings.Builder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Processor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<LocalBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TransportMessageFactory>(DependencyLifecycle.SingleInstance);
            
            return config;
        }

        public static IConfigureEndpoint UseDefermentBus(this IConfigureEndpoint config) 
        {
            Settings.Builder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<DefermentProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TransportMessageFactory>(DependencyLifecycle.SingleInstance);

            return config;
        }
    }
}
