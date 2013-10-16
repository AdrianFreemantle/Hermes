using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Deferment;

namespace Hermes.Messaging
{
    internal static class UnicastBusDependancyRegistrar
    {
        public static void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<TransportMessageFactory>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<CallBackManager>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<LocalBus>(DependencyLifecycle.SingleInstance);

            containerBuilder.RegisterType<Dispatcher>(DependencyLifecycle.InstancePerUnitOfWork);
            containerBuilder.RegisterType<IncomingMessageProcessor>(DependencyLifecycle.InstancePerUnitOfWork);

            if (!Settings.IsClientEndpoint)
            {
                containerBuilder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<OutgoingMessagesUnitOfWork>(DependencyLifecycle.InstancePerUnitOfWork);
            }
        }
    }

    internal static class DefermentBusDependancyRegistrar
    {
        public static void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DefermentProcessor>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<TransportMessageFactory>(DependencyLifecycle.SingleInstance);

            containerBuilder.RegisterType<Dispatcher>(DependencyLifecycle.InstancePerUnitOfWork);
        }
    }
}
