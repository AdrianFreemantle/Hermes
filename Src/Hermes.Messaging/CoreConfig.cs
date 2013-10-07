using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Deferment;

namespace Hermes.Messaging
{
    public static class CoreConfig
    {
        public static IConfigureEndpoint UseUnicastBus(this IConfigureEndpoint config)
        {
            Settings.Builder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TransportMessageFactory>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<CallBackManager>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<IncomingMessageProcessor>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<LocalBus>(DependencyLifecycle.SingleInstance);

            if (!Settings.IsClientEndpoint)
            {
                Settings.Builder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
                Settings.Builder.RegisterType<OutgoingMessagesUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            }
            
            return config;
        }

        public static IConfigureEndpoint UseDefermentBus(this IConfigureEndpoint config) 
        {
            Settings.Builder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<DefermentProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<TransportMessageFactory>(DependencyLifecycle.SingleInstance);

            return config;
        }
    }
}
