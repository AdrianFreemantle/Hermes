using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Deferment;

namespace Hermes.Messaging
{
    internal class UnicastBusDependancyRegistrar : IRegisterDependancies
    {
        public void Register(IContainerBuilder containerBuilder)
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
            containerBuilder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);

            if (!Settings.IsClientEndpoint)
            {
                containerBuilder.RegisterType<OutgoingMessagesUnitOfWork>(DependencyLifecycle.InstancePerUnitOfWork);
                containerBuilder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
